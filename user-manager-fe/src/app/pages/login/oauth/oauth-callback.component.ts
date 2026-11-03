import { Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';
import { TranslateService } from '../../../core/services/translate.service';
import { UNREACHABLE_SERVER_MESSAGE_VI, getHttpErrorMessage } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  imports: [RouterLink, AuthShellComponent],
  template: `
    <app-auth-shell>
      <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ translate.t().oauthCallbackTitle }}</h1>
      <p class="mt-1.5 text-sm text-fg-2">{{ translate.t().oauthCallbackSubtitle }}</p>
      <div class="mt-6">
        @if (error()) {
          <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
          <a routerLink="/login" class="btn-secondary btn-block mt-4">{{ translate.t().backToSignIn }}</a>
        } @else if (token()) {
          <div class="alert-ok" role="status"><span class="material-symbols-outlined">check_circle</span><span>{{ translate.t().oauthCallbackSuccess }}</span></div>
        } @else {
          <div class="flex items-center gap-3 text-sm text-fg-2" role="status"><span class="spinner text-brand-text"></span>{{ translate.t().oauthCallbackLoading }}</div>
        }
      </div>
    </app-auth-shell>
  `
})
export class OauthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);
  readonly translate = inject(TranslateService);
  readonly error = signal('');
  readonly token = signal('');

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    const state = this.route.snapshot.queryParamMap.get('state');
    const oauthError = this.route.snapshot.queryParamMap.get('error');
    const context = this.oauth.readAuthorizeContext();

    this.location.replaceState('/oauth/callback');

    if (oauthError) {
      this.error.set(`${this.translate.t().oauthCallbackLoginFailed} ${oauthError}`);
      this.clearHostedSessionIfPresent();
      this.oauth.clearAuthorizeContext();
      return;
    }

    if (!code || !context?.clientId || !context.redirectUri || !context.codeVerifier) {
      this.error.set(this.translate.t().oauthCallbackMissingContext);
      this.clearHostedSessionIfPresent();
      return;
    }

    if (context.state && state !== context.state) {
      this.error.set(this.translate.t().oauthCallbackStateMismatch);
      this.clearHostedSessionIfPresent();
      this.oauth.clearAuthorizeContext();
      return;
    }

    this.oauth
      .exchangeToken({
        grantType: 'authorization_code',
        clientId: context.clientId,
        redirectUri: context.redirectUri,
        code,
        codeVerifier: context.codeVerifier
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data?.accessToken && res.data.user) {
            // OIDC: the ID token must echo the nonce we generated for this authorization request.
            if (context.nonce && res.data.idToken && this.readNonce(res.data.idToken) !== context.nonce) {
              this.error.set(this.translate.t().oauthCallbackStateMismatch);
              this.oauth.clearAuthorizeContext();
              return;
            }
            this.auth.setSession({
              accessToken: res.data.accessToken,
              refreshToken: res.data.refreshToken,
              expiresAt: new Date(Date.now() + res.data.expiresIn * 1000).toISOString(),
              user: {
                ...res.data.user,
                permissions: []
              },
              applications: []
            });
            this.token.set(res.data.accessToken);
            this.oauth.clearAuthorizeContext();
            const nextUrl = context.returnUrl?.startsWith('/') ? context.returnUrl : '/';
            this.router.navigateByUrl(nextUrl, { replaceUrl: true });
            return;
          }

          this.error.set(res.error?.message || res.message || this.translate.t().oauthCallbackFailed);
          this.clearHostedSessionIfPresent();
        },
        error: (err: any) => {
          this.error.set(getHttpErrorMessage(err, this.translate.t().oauthCallbackFailed, UNREACHABLE_SERVER_MESSAGE_VI));
          this.clearHostedSessionIfPresent();
        }
      });
  }

  private clearHostedSessionIfPresent(): void {
    if (this.auth.isHostedSsoSession()) {
      this.auth.discardSession();
    }
  }

  private readNonce(idToken: string): string | null {
    try {
      const payload = idToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      return (JSON.parse(atob(payload.padEnd(payload.length + ((4 - (payload.length % 4)) % 4), '='))) as { nonce?: string }).nonce ?? null;
    } catch {
      return null;
    }
  }
}
