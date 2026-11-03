import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateService, type Locale } from '../../../core/services/translate.service';
import { NotificationComponent } from '../notification/notification.component';

/** Split-screen layout for every unauthenticated page (sign-in, MFA, reset, consent…). */
@Component({
  selector: 'app-auth-shell',
  standalone: true,
  imports: [RouterLink, NotificationComponent],
  template: `
    <div class="flex min-h-dvh bg-surface">
      <!-- Brand panel: fixed dark ink in both themes -->
      <aside class="relative hidden w-[44%] max-w-[640px] flex-col justify-between overflow-hidden bg-[#0b0c14] p-10 text-white lg:flex xl:p-14">
        <div class="pointer-events-none absolute inset-0"
             style="background-image: linear-gradient(rgb(255 255 255 / 0.045) 1px, transparent 1px), linear-gradient(90deg, rgb(255 255 255 / 0.045) 1px, transparent 1px); background-size: 44px 44px; mask-image: radial-gradient(ellipse 80% 70% at 30% 40%, black, transparent 80%);"></div>
        <a routerLink="/" class="relative flex items-center gap-2.5">
          <span class="flex h-9 w-9 items-center justify-center rounded-lg bg-brand text-white ring-1 ring-inset ring-white/15">
            <span class="material-symbols-outlined icon-fill text-[20px]">shield_lock</span>
          </span>
          <span class="text-[15px] font-semibold">{{ t().productName }}</span>
        </a>

        <div class="relative max-w-md">
          <h2 class="text-[32px] font-semibold leading-[1.15] tracking-[-0.02em] text-white">{{ t().authHeroTitle }}</h2>
          <p class="mt-4 text-[15px] leading-relaxed text-white/65">{{ t().authHeroBody }}</p>
          <ul class="mt-10 space-y-4">
            @for (point of heroPoints(); track point.text) {
              <li class="flex items-center gap-3.5 text-sm text-white/85">
                <span class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-white/[0.06] ring-1 ring-inset ring-white/10">
                  <span class="material-symbols-outlined text-[18px] text-[#a5aaff]">{{ point.icon }}</span>
                </span>
                {{ point.text }}
              </li>
            }
          </ul>
        </div>

        <div class="relative">
          <p class="text-2xs font-semibold uppercase tracking-[0.1em] text-white/40">{{ t().authStandardsLabel }}</p>
          <ul class="mt-2.5 flex flex-wrap gap-1.5">
            @for (std of standards; track std) {
              <li class="rounded-md bg-white/[0.06] px-2 py-1 font-mono text-[11px] text-white/70 ring-1 ring-inset ring-white/10">{{ std }}</li>
            }
          </ul>
        </div>
      </aside>

      <!-- Form column -->
      <main class="relative flex flex-1 flex-col">
        <div class="flex items-center justify-between px-5 py-4 sm:px-8">
          <a routerLink="/" class="flex items-center gap-2 lg:invisible">
            <span class="flex h-8 w-8 items-center justify-center rounded-lg bg-brand text-brand-fg">
              <span class="material-symbols-outlined icon-fill text-[18px]">shield_lock</span>
            </span>
            <span class="text-sm font-semibold">{{ t().productName }}</span>
          </a>
          <div class="flex items-center gap-1">
            <div class="segmented" role="radiogroup" [attr.aria-label]="t().language">
              @for (l of locales; track l) {
                <button type="button" role="radio" [attr.aria-checked]="translate.locale() === l" class="uppercase" [class.is-active]="translate.locale() === l" (click)="translate.setLocale(l)">{{ l }}</button>
              }
            </div>
            <button type="button" class="btn-icon" (click)="theme.toggleTheme()" [attr.aria-label]="t().toggleTheme">
              <span class="material-symbols-outlined">{{ theme.theme() === 'dark' ? 'light_mode' : 'dark_mode' }}</span>
            </button>
          </div>
        </div>

        <div class="flex flex-1 items-center justify-center px-5 pb-12 pt-4 sm:px-8">
          <div class="w-full animate-fade-up" [class]="wide() ? 'max-w-lg' : 'max-w-[400px]'">
            <ng-content />
          </div>
        </div>
      </main>
    </div>
    <app-notification />
  `
})
export class AuthShellComponent {
  readonly translate = inject(TranslateService);
  readonly theme = inject(ThemeService);
  readonly t = this.translate.t;
  readonly locales: Locale[] = ['en', 'vi', 'zh'];
  /** Protocols this server actually implements (see README → Features). */
  readonly standards = ['OAuth 2.0', 'OpenID Connect', 'PKCE · RFC 7636', 'TOTP · RFC 6238', 'Revocation · RFC 7009', 'Back-Channel Logout'];
  readonly heroPoints = computed(() => {
    const t = this.t();
    return [
      { icon: 'key', text: t.authHeroPoint1 },
      { icon: 'phonelink_lock', text: t.authHeroPoint2 },
      { icon: 'devices', text: t.authHeroPoint3 }
    ];
  });
  wide = input(false);
}
