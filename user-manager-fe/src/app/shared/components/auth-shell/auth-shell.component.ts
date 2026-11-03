import { Component, inject, input } from '@angular/core';
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
    <div class="flex min-h-dvh bg-canvas">
      <!-- Brand panel -->
      <aside class="relative hidden w-[44%] max-w-[640px] flex-col justify-between overflow-hidden border-r border-line bg-surface p-10 lg:flex xl:p-14">
        <div class="pointer-events-none absolute inset-0 opacity-[0.55] dark:opacity-30"
             style="background-image: radial-gradient(rgb(var(--line-strong)) 1px, transparent 1px); background-size: 22px 22px; mask-image: linear-gradient(to bottom, black, transparent 75%);"></div>
        <a routerLink="/" class="relative flex items-center gap-2.5">
          <span class="flex h-9 w-9 items-center justify-center rounded-lg bg-brand text-brand-fg shadow-xs">
            <span class="material-symbols-outlined icon-fill text-[20px]">shield_lock</span>
          </span>
          <span class="text-[15px] font-semibold text-fg">{{ t().productName }}</span>
        </a>

        <div class="relative max-w-md">
          <h2 class="text-[28px] font-semibold leading-tight tracking-tight text-fg">{{ t().authHeroTitle }}</h2>
          <p class="mt-3 text-[15px] leading-relaxed text-fg-2">{{ t().authHeroBody }}</p>
          <ul class="mt-8 space-y-3">
            @for (point of [t().authHeroPoint1, t().authHeroPoint2, t().authHeroPoint3]; track point) {
              <li class="flex items-center gap-3 text-sm text-fg">
                <span class="flex h-6 w-6 items-center justify-center rounded-full bg-brand-soft text-brand-text">
                  <span class="material-symbols-outlined text-[16px]">check</span>
                </span>
                {{ point }}
              </li>
            }
          </ul>
        </div>

        <p class="relative text-xs text-fg-3">{{ t().authSecureFooter }}</p>
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
  wide = input(false);
}
