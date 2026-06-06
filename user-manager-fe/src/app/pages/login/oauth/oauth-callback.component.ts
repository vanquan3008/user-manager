import { CommonModule, Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateService } from '../../../core/services/translate.service';
import { UNREACHABLE_SERVER_MESSAGE_VI, getHttpErrorMessage } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule],
  template: `
    <div class="min-h-screen overflow-hidden">
      <div
        class="relative flex min-h-screen items-center justify-center px-4 py-8 sm:px-6 lg:px-10"
        [ngClass]="theme() === 'dark' ? 'bg-[#081320] text-slate-100' : 'bg-[#f2f8ff] text-slate-900'"
      >
        <div class="pointer-events-none absolute inset-0 overflow-hidden">
          <div class="absolute left-[-7rem] top-[-6rem] h-72 w-72 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-sky-500/18' : 'bg-sky-300/40'"></div>
          <div class="absolute bottom-[-10rem] right-[-6rem] h-80 w-80 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-blue-400/12' : 'bg-blue-200/40'"></div>
          <div class="absolute inset-x-0 top-0 h-72 bg-[radial-gradient(circle_at_top,rgba(96,165,250,0.18),transparent_55%)]"></div>
        </div>

        <div
          class="relative w-full max-w-xl overflow-hidden rounded-[32px] border shadow-2xl"
          [ngClass]="theme() === 'dark' ? 'border-slate-700/70 bg-slate-950/85 shadow-black/40' : 'border-white/70 bg-white/92 shadow-slate-900/10'"
        >
          <section class="space-y-6 px-6 py-7 sm:px-8 sm:py-8">
            <div>
              <p class="text-[12px] font-semibold uppercase tracking-[0.18em]" [ngClass]="theme() === 'dark' ? 'text-sky-200/80' : 'text-sky-800/75'">
                {{ translate.t().oauthCallbackTitle }}
              </p>
              <h1 class="mt-2 text-3xl font-semibold tracking-tight">{{ translate.t().oauthCallbackTitle }}</h1>
              <p class="mt-2 text-sm leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-600'">
                {{ translate.t().oauthCallbackSubtitle }}
              </p>
            </div>

            @if (error()) {
              <div class="rounded-[1.6rem] border px-4 py-4 text-sm" [ngClass]="theme() === 'dark' ? 'border-rose-500/40 bg-rose-500/10 text-rose-100' : 'border-rose-300 bg-rose-50 text-rose-700'">
                <div class="flex items-start gap-3">
                  <span class="material-symbols-outlined mt-0.5 text-[20px]">error</span>
                  <span class="leading-6">{{ error() }}</span>
                </div>
              </div>
            } @else if (token()) {
              <div class="rounded-[1.6rem] border px-4 py-4 text-sm" [ngClass]="theme() === 'dark' ? 'border-emerald-500/35 bg-emerald-500/10 text-emerald-100' : 'border-emerald-300 bg-emerald-50 text-emerald-700'">
                <div class="flex items-start gap-3">
                  <span class="material-symbols-outlined mt-0.5 text-[20px]">check_circle</span>
                  <span class="leading-6">{{ translate.t().oauthCallbackSuccess }}</span>
                </div>
              </div>
            } @else {
              <div class="rounded-[1.6rem] border px-5 py-6" [ngClass]="theme() === 'dark' ? 'border-slate-700 bg-slate-900/70' : 'border-slate-200 bg-slate-50/80'">
                <div class="flex items-center gap-4">
                  <span class="inline-block h-10 w-10 animate-spin rounded-full border-[3px] border-sky-300/20 border-t-sky-400"></span>
                  <div>
                    <p class="text-base font-semibold">{{ translate.t().oauthCallbackLoading }}</p>
                    <p class="mt-1 text-sm" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">
                      {{ translate.t().oauthCallbackSubtitle }}
                    </p>
                  </div>
                </div>
              </div>
            }
          </section>
        </div>
      </div>
    </div>
  `
})
export class OauthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
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
}
