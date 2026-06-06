import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { OAuthAuthorizeRequest, OAuthAuthorizeResponse } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateService } from '../../../core/services/translate.service';
import { UNREACHABLE_SERVER_MESSAGE_VI, getHttpErrorMessage } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-authorize',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule],
  template: `
    <div class="min-h-[100dvh] overflow-hidden font-['Inter','SF_Pro_Display','Segoe_UI',sans-serif]">
      <div
        class="relative flex min-h-[100dvh] items-center justify-center px-4 py-8 sm:px-6 lg:px-10"
        [ngClass]="theme() === 'dark' ? 'bg-[#081320] text-slate-100' : 'bg-[#f2f8ff] text-slate-900'"
      >
        <div class="pointer-events-none absolute inset-0 overflow-hidden">
          <div class="absolute inset-0" [ngClass]="theme() === 'dark' ? 'bg-[radial-gradient(circle_at_top_left,rgba(59,130,246,0.18),transparent_30%),radial-gradient(circle_at_85%_15%,rgba(99,102,241,0.2),transparent_28%),linear-gradient(145deg,#040b16,#071120_42%,#0a1730_100%)]' : 'bg-[radial-gradient(circle_at_top_left,rgba(59,130,246,0.14),transparent_28%),radial-gradient(circle_at_85%_15%,rgba(6,182,212,0.12),transparent_30%),linear-gradient(180deg,#f8fbff,#eef5ff)]'"></div>
          <div class="absolute left-[-7rem] top-[-6rem] h-72 w-72 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-sky-500/22' : 'bg-sky-300/40'"></div>
          <div class="absolute bottom-[-10rem] right-[-6rem] h-80 w-80 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-cyan-400/12' : 'bg-blue-200/40'"></div>
          <div
            class="absolute inset-0 opacity-[0.08]"
            [ngClass]="theme() === 'dark' ? 'bg-[linear-gradient(rgba(255,255,255,0.16)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.16)_1px,transparent_1px)]' : 'bg-[linear-gradient(rgba(15,23,42,0.18)_1px,transparent_1px),linear-gradient(90deg,rgba(15,23,42,0.18)_1px,transparent_1px)]'"
            style="background-size: 40px 40px;"
          ></div>
        </div>

        <div
          class="relative grid w-full max-w-7xl overflow-hidden rounded-[36px] border shadow-2xl backdrop-blur-xl lg:grid-cols-[0.92fr_1.08fr]"
          [ngClass]="theme() === 'dark' ? 'border-white/10 bg-slate-950/72 shadow-black/45' : 'border-white/80 bg-white/78 shadow-slate-900/10'"
        >
          <section
            class="relative overflow-hidden px-6 py-7 sm:px-8 sm:py-8 lg:px-10 lg:py-10"
            [ngClass]="theme() === 'dark' ? 'bg-[linear-gradient(160deg,rgba(7,18,31,0.96),rgba(9,17,31,0.9))]' : 'bg-[linear-gradient(160deg,rgba(245,249,255,0.96),rgba(231,241,255,0.9))]'"
          >
            <div class="pointer-events-none absolute inset-0">
              <div class="absolute -left-24 top-10 h-56 w-56 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-indigo-500/18' : 'bg-sky-300/35'"></div>
              <div class="absolute bottom-[-5rem] right-[-4rem] h-72 w-72 rounded-full blur-3xl" [ngClass]="theme() === 'dark' ? 'bg-cyan-400/10' : 'bg-cyan-200/40'"></div>
            </div>

            <div class="relative flex h-full flex-col justify-between gap-8">
              <div class="space-y-7">
                <div
                  class="inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-[11px] font-semibold uppercase tracking-[0.22em]"
                  [ngClass]="theme() === 'dark' ? 'border-cyan-400/30 bg-cyan-400/10 text-cyan-100' : 'border-sky-500/25 bg-sky-500/10 text-sky-800'"
                >
                  <span class="h-2 w-2 rounded-full bg-emerald-400"></span>
                  <span>{{ translate.t().oauthAuthorizeEyebrow }}</span>
                </div>

                <div class="space-y-4">
                  <div class="max-w-xl">
                    <p class="text-[12px] font-semibold uppercase tracking-[0.26em]" [ngClass]="theme() === 'dark' ? 'text-cyan-200/85' : 'text-sky-800/70'">
                      Social CRM workspace
                    </p>
                    <h1 class="mt-3 max-w-lg text-3xl font-semibold tracking-tight sm:text-4xl lg:text-[3rem] lg:leading-[1.02]">
                      {{ translate.t().oauthAuthorizeTitle }}
                    </h1>
                    <p class="mt-3 max-w-lg text-sm leading-7 sm:text-[15px]" [ngClass]="theme() === 'dark' ? 'text-slate-300' : 'text-slate-600'">
                      {{ translate.t().oauthAuthorizeSubtitle }}
                    </p>
                  </div>

                  <div
                    class="rounded-[28px] border p-5 backdrop-blur-xl"
                    [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.035]' : 'border-white/80 bg-white/88'"
                  >
                    <div class="flex flex-col gap-5 sm:flex-row sm:items-center">
                      <div class="flex items-center gap-4">
                        <div class="flex h-16 w-16 shrink-0 items-center justify-center rounded-[22px] bg-[linear-gradient(135deg,#38bdf8,#4f46e5)] text-2xl font-semibold text-white shadow-[0_18px_50px_rgba(56,189,248,0.25)]">
                          {{ appName().charAt(0).toUpperCase() }}
                        </div>
                        <div class="min-w-0">
                          <div class="flex flex-wrap items-center gap-2">
                            <p class="truncate text-xl font-semibold">{{ appName() }}</p>
                            <span
                              class="inline-flex items-center rounded-full px-2.5 py-1 text-[10px] font-semibold uppercase tracking-[0.18em]"
                              [ngClass]="theme() === 'dark' ? 'bg-emerald-500/15 text-emerald-200' : 'bg-emerald-50 text-emerald-700'"
                            >
                              Connected
                            </span>
                          </div>
                          <p class="mt-1 text-sm" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">{{ clientId() }}</p>
                        </div>
                      </div>

                      <div class="sm:ml-auto">
                        <div
                          class="inline-flex items-center gap-2 rounded-full border px-3 py-2 text-xs"
                          [ngClass]="theme() === 'dark' ? 'border-slate-700 bg-slate-900/70 text-slate-300' : 'border-slate-200 bg-slate-50 text-slate-600'"
                        >
                          <span class="material-symbols-outlined text-[16px]">verified_user</span>
                          <span>{{ missingScopes().length === 0 ? 'Trusted session' : 'Approval required' }}</span>
                        </div>
                      </div>
                    </div>

                    <div class="mt-5 grid gap-3 sm:grid-cols-3">
                      <div class="rounded-2xl border px-4 py-4 transition duration-200 hover:-translate-y-0.5" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-slate-950/65' : 'border-slate-100 bg-slate-50/90'">
                        <p class="text-[11px] uppercase tracking-[0.18em]" [ngClass]="theme() === 'dark' ? 'text-slate-500' : 'text-slate-400'">{{ translate.t().oauthAuthorizeRequestedScopes }}</p>
                        <p class="mt-2 text-2xl font-semibold">{{ requestedScopes().length }}</p>
                      </div>
                      <div class="rounded-2xl border px-4 py-4 transition duration-200 hover:-translate-y-0.5" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-slate-950/65' : 'border-slate-100 bg-slate-50/90'">
                        <p class="text-[11px] uppercase tracking-[0.18em]" [ngClass]="theme() === 'dark' ? 'text-slate-500' : 'text-slate-400'">{{ translate.t().oauthAuthorizeAlreadyGranted }}</p>
                        <p class="mt-2 text-2xl font-semibold">{{ grantedScopes().length }}</p>
                      </div>
                      <div class="rounded-2xl border px-4 py-4 transition duration-200 hover:-translate-y-0.5" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-slate-950/65' : 'border-slate-100 bg-slate-50/90'">
                        <p class="text-[11px] uppercase tracking-[0.18em]" [ngClass]="theme() === 'dark' ? 'text-slate-500' : 'text-slate-400'">{{ translate.t().oauthAuthorizeNewAccess }}</p>
                        <p class="mt-2 text-2xl font-semibold">{{ missingScopes().length }}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="grid gap-4 sm:grid-cols-[1.05fr_0.95fr]">
                <div class="rounded-[26px] border p-5" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-slate-950/65' : 'border-white/80 bg-white/92'">
                  <div class="flex items-start gap-3">
                    <span class="material-symbols-outlined mt-0.5 text-[20px]" [ngClass]="theme() === 'dark' ? 'text-cyan-300' : 'text-sky-600'">verified_user</span>
                    <div>
                      <p class="text-sm font-semibold">Review before you continue</p>
                      <p class="mt-2 text-sm leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-300' : 'text-slate-600'">
                        {{ translate.t().oauthAuthorizeReviewNote }}
                      </p>
                    </div>
                  </div>
                </div>

                <div class="rounded-[26px] border p-5" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.03]' : 'border-white/80 bg-white/88'">
                  <p class="text-[11px] uppercase tracking-[0.2em]" [ngClass]="theme() === 'dark' ? 'text-slate-500' : 'text-slate-400'">{{ translate.t().oauthAuthorizeRedirectLabel }}</p>
                  <p class="mt-3 text-base font-semibold">{{ redirectHost() }}</p>
                  <p class="mt-2 break-all text-xs leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">{{ redirectUri() }}</p>
                </div>
              </div>
            </div>
          </section>

          <section class="relative flex flex-col justify-center px-5 py-6 sm:px-8 sm:py-8 lg:px-10" [ngClass]="theme() === 'dark' ? 'bg-[linear-gradient(180deg,rgba(2,9,20,0.88),rgba(6,13,25,0.96))]' : 'bg-white/96'">
            <div class="mx-auto w-full max-w-3xl">
              @if (error()) {
                <div class="rounded-[1.75rem] border px-5 py-5 text-sm shadow-lg" [ngClass]="theme() === 'dark' ? 'border-rose-500/40 bg-rose-500/10 text-rose-100 shadow-rose-950/30' : 'border-rose-300 bg-rose-50 text-rose-700 shadow-rose-100/60'">
                  <div class="flex items-start gap-3">
                    <span class="material-symbols-outlined mt-0.5 text-[20px]">error</span>
                    <span class="leading-6">{{ error() }}</span>
                  </div>
                </div>
              } @else if (loading()) {
                <div class="rounded-[2rem] border px-6 py-10 text-center shadow-lg" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.035] shadow-black/20' : 'border-slate-200 bg-slate-50/80 shadow-slate-200/80'">
                  <span class="mx-auto inline-block h-10 w-10 animate-spin rounded-full border-[3px] border-sky-300/20 border-t-sky-400"></span>
                  <p class="mt-4 text-base font-semibold">{{ translate.t().oauthAuthorizeLoading }}</p>
                </div>
              } @else if (consentRequired() && authorizeResponse()) {
                <div class="space-y-5">
                  <div class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
                    <div class="max-w-xl">
                      <p class="text-[12px] font-semibold uppercase tracking-[0.22em]" [ngClass]="theme() === 'dark' ? 'text-cyan-200/80' : 'text-sky-700/80'">
                        {{ appName() }}
                      </p>
                      <h2 class="mt-2 text-3xl font-semibold tracking-tight">{{ translate.t().oauthAuthorizeRequestedScopes }}</h2>
                      <p class="mt-2 text-sm leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">
                        {{ missingScopes().length === 0 ? 'All requested scopes were previously approved for this workspace.' : 'New scopes are highlighted so you can review exactly what changes before continuing.' }}
                      </p>
                    </div>

                    <div class="flex flex-wrap gap-3">
                      <div
                        class="rounded-2xl border px-3.5 py-3 text-left"
                        [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.035] text-slate-200' : 'border-slate-200 bg-slate-50 text-slate-700'"
                      >
                        <div class="text-lg font-semibold">{{ requestedScopes().length }}</div>
                        <div class="text-[10px] uppercase tracking-[0.18em]" [ngClass]="theme() === 'dark' ? 'text-slate-500' : 'text-slate-400'">Scopes</div>
                      </div>
                      <div
                        class="rounded-2xl border px-3.5 py-3 text-left"
                        [ngClass]="theme() === 'dark' ? 'border-cyan-400/20 bg-cyan-400/10 text-cyan-100' : 'border-sky-200 bg-sky-50 text-sky-700'"
                      >
                        <div class="text-lg font-semibold">{{ missingScopes().length }}</div>
                        <div class="text-[10px] uppercase tracking-[0.18em]">New access</div>
                      </div>
                    </div>
                  </div>

                  <div
                    class="rounded-[26px] border p-4"
                    [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.03]' : 'border-slate-200 bg-slate-50/85'"
                  >
                    <div class="flex items-start gap-3">
                      <div class="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl" [ngClass]="theme() === 'dark' ? 'bg-cyan-400/10 text-cyan-200' : 'bg-sky-100 text-sky-700'">
                        <span class="material-symbols-outlined text-[20px]">privacy_tip</span>
                      </div>
                      <div>
                        <p class="text-sm font-semibold">{{ redirectHost() }}</p>
                        <p class="mt-1 text-sm leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">
                          The authorization code will return only to this registered redirect address.
                        </p>
                      </div>
                    </div>
                  </div>

                  <ul class="grid gap-3 sm:grid-cols-2">
                    @for (scope of requestedScopes(); track scope) {
                      <li
                        class="group rounded-[1.6rem] border p-4 transition duration-200 hover:-translate-y-0.5 hover:shadow-[0_20px_40px_rgba(8,15,27,0.12)]"
                        [ngClass]="theme() === 'dark'
                          ? (isScopeGranted(scope) ? 'border-white/10 bg-slate-950/70 hover:border-white/20' : 'border-cyan-400/18 bg-[linear-gradient(180deg,rgba(34,211,238,0.08),rgba(15,23,42,0.82))] hover:border-cyan-300/30')
                          : (isScopeGranted(scope) ? 'border-slate-200 bg-slate-50/80 hover:border-slate-300' : 'border-sky-200 bg-sky-50/80 hover:border-sky-300')"
                      >
                        <div class="flex items-start gap-3">
                          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl text-base font-semibold" [ngClass]="theme() === 'dark'
                            ? (isScopeGranted(scope) ? 'bg-white/[0.06] text-slate-200' : 'bg-cyan-400/12 text-cyan-100')
                            : (isScopeGranted(scope) ? 'bg-slate-200/70 text-slate-700' : 'bg-sky-100 text-sky-700')">
                            <span class="material-symbols-outlined text-[20px]">{{ scopeIcon(scope) }}</span>
                          </div>

                          <div class="min-w-0 flex-1">
                            <div class="flex items-start justify-between gap-3">
                              <div class="min-w-0">
                                <p class="truncate text-sm font-semibold">{{ formatScopeTitle(scope) }}</p>
                                <p class="mt-1 text-sm leading-6" [ngClass]="theme() === 'dark' ? 'text-slate-400' : 'text-slate-500'">
                                  {{ scopeDescription(scope) }}
                                </p>
                              </div>
                              <span
                                class="shrink-0 rounded-full px-2.5 py-1 text-[11px] font-semibold"
                                [ngClass]="isScopeGranted(scope)
                                  ? (theme() === 'dark' ? 'bg-emerald-500/15 text-emerald-200' : 'bg-emerald-50 text-emerald-700')
                                  : (theme() === 'dark' ? 'bg-cyan-400/15 text-cyan-100' : 'bg-sky-50 text-sky-700')"
                              >
                                {{ isScopeGranted(scope) ? translate.t().oauthAuthorizeGrantedBadge : translate.t().oauthAuthorizeRequestedBadge }}
                              </span>
                            </div>

                            <div class="mt-3 flex flex-wrap items-center gap-2 text-[11px]">
                              <span class="rounded-full px-2.5 py-1" [ngClass]="theme() === 'dark' ? 'bg-white/[0.05] text-slate-300' : 'bg-white text-slate-600'">
                                {{ scope }}
                              </span>
                              <span class="rounded-full px-2.5 py-1" [ngClass]="theme() === 'dark' ? 'bg-slate-900 text-slate-400' : 'bg-slate-100 text-slate-500'">
                                {{ scopeCategory(scope) }}
                              </span>
                            </div>
                          </div>
                        </div>
                      </li>
                    }
                  </ul>

                  <div class="flex flex-col-reverse gap-3 border-t pt-5" [ngClass]="theme() === 'dark' ? 'border-white/10' : 'border-slate-200'">
                    <button
                      type="button"
                      class="inline-flex h-12 items-center justify-center rounded-full border px-5 text-sm font-semibold transition duration-200"
                      [ngClass]="theme() === 'dark' ? 'border-rose-500/35 bg-rose-500/10 text-rose-100 hover:bg-rose-500/15' : 'border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100'"
                      (click)="deny()"
                    >
                      {{ translate.t().oauthAuthorizeDeny }}
                    </button>
                    <button
                      type="button"
                      class="inline-flex h-12 items-center justify-center rounded-full bg-[linear-gradient(135deg,#22d3ee,#3b82f6,#4f46e5)] px-5 text-sm font-semibold text-white shadow-[0_18px_40px_rgba(37,99,235,0.35)] transition duration-200 hover:-translate-y-0.5 hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-70"
                      [disabled]="loading()"
                      (click)="approve()"
                    >
                      {{ translate.t().oauthAuthorizeApprove }}
                    </button>
                  </div>
                </div>
              } @else {
                <div class="rounded-[1.75rem] border px-6 py-8 text-center shadow-lg" [ngClass]="theme() === 'dark' ? 'border-white/10 bg-white/[0.035] shadow-black/20' : 'border-slate-200 bg-slate-50/80 shadow-slate-200/80'">
                  <span class="mx-auto inline-flex h-12 w-12 items-center justify-center rounded-full" [ngClass]="theme() === 'dark' ? 'bg-emerald-500/15 text-emerald-200' : 'bg-emerald-50 text-emerald-700'">
                    <span class="material-symbols-outlined">north_east</span>
                  </span>
                  <p class="mt-4 text-base font-semibold">{{ translate.t().oauthAuthorizeRedirecting }}</p>
                </div>
              }
            </div>
          </section>
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
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
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
    if (!request) return;
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
