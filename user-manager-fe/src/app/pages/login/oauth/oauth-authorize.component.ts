import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import type { OAuthAuthorizeRequest, OAuthAuthorizeResponse } from '../../../core/models/api-response';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-authorize',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full overflow-y-auto">
      <div class="min-h-full flex items-center justify-center bg-slate-950 bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 px-4 py-8">
        <div class="w-full max-w-xl rounded-2xl border border-slate-700/80 bg-slate-900/80 px-6 py-5 shadow-2xl shadow-slate-900/80">
          <h1 class="mb-2 text-lg font-semibold text-slate-50">OAuth Authorization</h1>
          @if (error()) {
            <p class="rounded-xl border border-rose-500/60 bg-rose-950/50 px-3 py-2 text-sm text-rose-300">
              {{ error() }}
            </p>
          } @else if (loading()) {
            <p class="text-sm text-slate-300">Đang xử lý yêu cầu ủy quyền...</p>
          } @else if (consentRequired() && authorizeResponse()) {
            <p class="text-sm text-slate-300 mb-3">
              Ứng dụng <span class="font-semibold text-slate-100">{{ authorizeResponse()!.clientName }}</span>
              yêu cầu các quyền sau:
            </p>
            <ul class="mb-4 space-y-1 text-sm text-slate-200">
              @for (scope of authorizeResponse()!.missingScopes; track scope) {
                <li class="rounded border border-slate-700 px-2 py-1">{{ scope }}</li>
              }
            </ul>
            <div class="flex gap-2 justify-end">
              <button type="button" (click)="deny()" class="rounded border border-rose-500/70 px-3 py-1.5 text-sm text-rose-200 hover:bg-rose-900/40">
                Từ chối
              </button>
              <button type="button" (click)="approve()" class="rounded border border-emerald-500/70 px-3 py-1.5 text-sm text-emerald-200 hover:bg-emerald-900/40">
                Đồng ý
              </button>
            </div>
          } @else {
            <p class="text-sm text-slate-300">Đang chuyển hướng...</p>
          }
        </div>
      </div>
    </div>
  `
})
export class OauthAuthorizeComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);

  readonly loading = signal(true);
  readonly error = signal('');
  readonly consentRequired = signal(false);
  readonly authorizeRequest = signal<OAuthAuthorizeRequest | null>(null);
  readonly authorizeResponse = signal<OAuthAuthorizeResponse | null>(null);

  ngOnInit(): void {
    const request = this.readAuthorizeRequest();
    if (!request) {
      return;
    }
    this.authorizeRequest.set(request);

    if (!this.auth.hasValidSession()) {
      const returnUrl = this.router.url;
      this.router.navigate(['/login'], { queryParams: { returnUrl } });
      return;
    }

    this.oauth.authorize(request).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.loading.set(false);
          this.error.set(res.message || res.error?.message || 'Authorize thất bại.');
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
      error: (err) => {
        this.loading.set(false);
        this.error.set(getHttpErrorMessage(err, 'Authorize thất bại.', UNREACHABLE_SERVER_MESSAGE_VI));
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
          this.error.set(res.message || res.error?.message || 'Consent thất bại.');
          return;
        }
        this.redirectToClient(request.redirectUri, { code: res.data.code, state: res.data.state ?? request.state ?? undefined });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getHttpErrorMessage(err, 'Consent thất bại.', UNREACHABLE_SERVER_MESSAGE_VI));
      }
    });
  }

  deny(): void {
    const request = this.authorizeRequest();
    if (!request) return;
    this.redirectToClient(request.redirectUri, { error: 'access_denied', state: request.state ?? undefined });
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

    if (!clientId || !redirectUri || !scopeRaw || !codeChallenge) {
      this.loading.set(false);
      this.error.set('Thiếu tham số bắt buộc: client_id, redirect_uri, scope, code_challenge.');
      return null;
    }

    const scopes = scopeRaw.split(/[,\s]+/).map(s => s.trim()).filter(Boolean);
    return {
      clientId,
      redirectUri,
      scopes,
      state,
      codeChallenge,
      codeChallengeMethod,
      prompt
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
