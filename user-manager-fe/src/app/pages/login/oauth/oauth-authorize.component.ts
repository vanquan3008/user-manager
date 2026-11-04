import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { OAuthAuthorizeRequest, OAuthAuthorizeResponse } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';
import { TranslateService } from '../../../core/services/translate.service';
import { UNREACHABLE_SERVER_MESSAGE_VI, getHttpErrorMessage } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-authorize',
  standalone: true,
  imports: [AuthShellComponent],
  template: `
    <app-auth-shell>
      @if (loading()) {
        <div class="flex flex-col items-center py-10 text-center" role="status">
          <span class="spinner h-6 w-6 text-brand-text"></span>
          <p class="mt-4 text-sm text-fg-2">{{ translate.t().oauthCallbackLoading }}</p>
        </div>
      } @else if (error()) {
        <div class="mb-6">
          <div class="mb-5 flex h-11 w-11 items-center justify-center rounded-xl bg-bad-soft text-bad"><span class="material-symbols-outlined">gpp_bad</span></div>
          <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ translate.t().oauthAuthorizeFailed }}</h1>
        </div>
        <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
      } @else if (consentRequired()) {
        <div class="mb-6 flex items-center gap-3">
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-brand text-lg font-semibold text-brand-fg">{{ appName().charAt(0).toUpperCase() }}</div>
          <span class="material-symbols-outlined text-fg-3">sync_alt</span>
          <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-surface-2 text-fg-2 ring-1 ring-inset ring-line">
            <span class="material-symbols-outlined icon-fill">shield_lock</span>
          </div>
        </div>
        <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ appName() }}</h1>
        <p class="mt-1.5 text-sm text-fg-2">{{ translate.t().oauthAuthorizeSubtitle }}</p>

        <div class="card mt-6 divide-y divide-line">
          @for (scope of requestedScopes(); track scope) {
            <div class="flex items-start gap-3 px-4 py-3">
              <span class="icon-tile h-8 w-8"><span class="material-symbols-outlined text-[18px]">{{ scopeIcon(scope) }}</span></span>
              <div class="min-w-0 flex-1">
                <div class="flex items-center gap-2">
                  <p class="text-sm font-medium">{{ formatScopeTitle(scope) }}</p>
                  @if (isScopeGranted(scope)) { <span class="badge-ok">{{ translate.t().oauthAuthorizeAlreadyGranted }}</span> }
                </div>
                <p class="mt-0.5 text-[13px] text-fg-2">{{ scopeDescription(scope) }}</p>
              </div>
            </div>
          }
        </div>

        <div class="mt-4 flex items-start gap-2 text-xs text-fg-3">
          <span class="material-symbols-outlined text-[16px]">link</span>
          <span>{{ translate.t().oauthAuthorizeRedirectLabel }}: <span class="font-medium text-fg-2">{{ redirectHost() }}</span></span>
        </div>
        <p class="mt-2 text-xs leading-relaxed text-fg-3">{{ translate.t().oauthAuthorizeReviewNote }}</p>

        <div class="mt-6 grid grid-cols-2 gap-2">
          <button type="button" class="btn-secondary btn-lg" (click)="deny()">{{ translate.t().oauthAuthorizeDeny }}</button>
          <button type="button" class="btn-primary btn-lg" (click)="approve()">{{ translate.t().oauthAuthorizeApprove }}</button>
        </div>
      }
    </app-auth-shell>
  `
})
export class OauthAuthorizeComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);
  readonly translate = inject(TranslateService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly consentRequired = signal(false);
  readonly authorizeRequest = signal<OAuthAuthorizeRequest | null>(null);
  readonly authorizeResponse = signal<OAuthAuthorizeResponse | null>(null);
  readonly requestedScopes = computed(() => {
    const response = this.authorizeResponse();
    if (response?.requestedScopes?.length) {
      return response.requestedScopes;
    }

    return this.authorizeRequest()?.scopes ?? [];
  });
  readonly grantedScopes = computed(() => this.authorizeResponse()?.alreadyGrantedScopes ?? []);
  readonly missingScopes = computed(() => this.authorizeResponse()?.missingScopes ?? this.requestedScopes());

  ngOnInit(): void {
    const request = this.readAuthorizeRequest();
    if (!request) {
      return;
    }
    this.authorizeRequest.set(request);

    if (!this.auth.hasHostedAuthSession()) {
      const returnUrl = this.router.url;
      this.router.navigate(['/sso-login'], { queryParams: { returnUrl } });
      return;
    }

    this.oauth.authorize(request).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.loading.set(false);
          this.error.set(res.message || res.error?.message || this.translate.t().oauthAuthorizeFailed);
          return;
        }

        this.authorizeResponse.set(res.data);
        if (res.data.code) {
          this.redirectToClient(request.redirectUri, { code: res.data.code, state: res.data.state ?? request.state ?? undefined });
          return;
        }

        this.consentRequired.set(!!res.data.requiresConsent);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(getHttpErrorMessage(err, this.translate.t().oauthAuthorizeFailed, UNREACHABLE_SERVER_MESSAGE_VI));
      }
    });
  }

  approve(): void {
    const request = this.authorizeRequest();
    if (!request) return;
    this.loading.set(true);

    this.oauth.consent({
      ...request,
      approve: true
    }).subscribe({
      next: (res) => {
        if (!res.success || !res.data?.code) {
          this.loading.set(false);
          this.error.set(res.message || res.error?.message || this.translate.t().oauthAuthorizeConsentFailed);
          return;
        }

        this.redirectToClient(request.redirectUri, { code: res.data.code, state: res.data.state ?? request.state ?? undefined });
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(getHttpErrorMessage(err, this.translate.t().oauthAuthorizeConsentFailed, UNREACHABLE_SERVER_MESSAGE_VI));
      }
    });
  }

  deny(): void {
    const request = this.authorizeRequest();
    // Only redirect back once the server has validated client_id/redirect_uri (prevents open redirects).
    if (!request || !this.authorizeResponse()) return;
    this.redirectToClient(request.redirectUri, { error: 'access_denied', state: request.state ?? undefined });
  }

  appName(): string {
    return this.authorizeResponse()?.clientName?.trim() || this.authorizeRequest()?.clientId?.trim() || 'Unknown application';
  }

  clientId(): string {
    return this.authorizeRequest()?.clientId?.trim() || 'client_id';
  }

  redirectUri(): string {
    return this.authorizeResponse()?.redirectUri?.trim() || this.authorizeRequest()?.redirectUri?.trim() || 'redirect_uri';
  }

  redirectHost(): string {
    try {
      return new URL(this.redirectUri()).host || this.redirectUri();
    } catch {
      return this.redirectUri();
    }
  }

  isScopeGranted(scope: string): boolean {
    return this.grantedScopes().includes(scope) && !this.missingScopes().includes(scope);
  }

  formatScopeTitle(scope: string): string {
    const readable = scope
      .split(/[.:]/)
      .filter(Boolean)
      .map((part) => part.replace(/[-_]+/g, ' '))
      .join(' / ')
      .trim();

    return readable.replace(/\b\w/g, (char) => char.toUpperCase()) || scope;
  }

  scopeDescription(scope: string): string {
    const normalized = scope.trim().toLowerCase();
    switch (normalized) {
      case 'openid':
        return 'Confirms your identity and allows the client to complete sign-in.';
      case 'profile':
        return 'Lets the client read your basic profile details for account setup.';
      case 'email':
        return 'Lets the client read your primary email address for contact and login.';
      default:
        return 'Allows this application to use the permission represented by this scope.';
    }
  }

  scopeCategory(scope: string): string {
    const normalized = scope.trim().toLowerCase();
    if (normalized === 'openid') return 'Identity';
    if (normalized === 'email') return 'Contact';
    if (normalized === 'profile') return 'Profile';
    return 'Access';
  }

  scopeIcon(scope: string): string {
    const normalized = scope.trim().toLowerCase();
    if (normalized === 'openid') return 'badge';
    if (normalized === 'email') return 'alternate_email';
    if (normalized === 'profile') return 'person';
    return 'key';
  }

  private readAuthorizeRequest(): OAuthAuthorizeRequest | null {
    const q = this.route.snapshot.queryParamMap;
    const clientId = q.get('client_id')?.trim() ?? '';
    const redirectUri = q.get('redirect_uri')?.trim() ?? '';
    const scopeRaw = q.get('scope')?.trim() ?? '';
    const state = q.get('state');
    const codeChallenge = q.get('code_challenge')?.trim() ?? '';
    const codeChallengeMethod = (q.get('code_challenge_method')?.trim() || 'S256') as 'S256' | 'plain';
    const prompt = q.get('prompt');
    const nonce = q.get('nonce');

    if (!clientId || !redirectUri || !scopeRaw || !codeChallenge) {
      this.loading.set(false);
      this.error.set(this.translate.t().oauthAuthorizeMissingParams);
      return null;
    }

    const scopes = scopeRaw.split(/[,\s]+/).map((scope) => scope.trim()).filter(Boolean);
    return {
      clientId,
      redirectUri,
      scopes,
      state,
      codeChallenge,
      codeChallengeMethod,
      prompt,
      nonce
    };
  }

  private redirectToClient(redirectUri: string, params: Record<string, string | undefined>): void {
    const url = new URL(redirectUri);
    for (const [key, value] of Object.entries(params)) {
      if (value) {
        url.searchParams.set(key, value);
      }
    }
    window.location.assign(url.toString());
  }
}
