import { CommonModule } from '@angular/common';
import { Component, input, output, signal } from '@angular/core';
import type { IdentityUser } from '../../../core/models';
import { IconButtonComponent } from '../../../shared/components/icon-button/icon-button.component';

@Component({
  selector: 'app-user-access-overview',
  standalone: true,
  imports: [CommonModule, IconButtonComponent],
  template: `
    <section class="relative overflow-hidden rounded-[2rem] border p-5 shadow-[0_24px_80px_-46px_rgba(14,165,233,0.42)] sm:p-6" [ngClass]="surfaceClasses()">
      <div class="pointer-events-none absolute inset-0">
        <div class="absolute left-0 top-0 h-40 w-40 rounded-full bg-cyan-400/14 blur-3xl"></div>
        <div class="absolute right-0 top-8 h-48 w-48 rounded-full bg-indigo-500/14 blur-3xl"></div>
      </div>

      <div class="relative">
        <div class="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div class="min-w-0 flex-1">
            <div class="flex min-w-0 flex-col gap-4 sm:flex-row sm:items-start">
              <div class="relative flex h-20 w-20 shrink-0 items-center justify-center rounded-[1.8rem] bg-gradient-to-br from-cyan-400 via-sky-500 to-indigo-600 text-2xl font-semibold text-white shadow-[0_22px_45px_-24px_rgba(34,211,238,0.7)] ring-1 ring-white/15">
                <span class="absolute inset-[1px] rounded-[1.7rem] bg-white/6"></span>
                <span class="relative">{{ (user().userName || user().email || '?').charAt(0).toUpperCase() }}</span>
              </div>

              <div class="min-w-0 flex-1">
                <div class="flex flex-wrap items-center gap-2">
                  <span class="inline-flex rounded-full border border-cyan-400/20 bg-cyan-400/12 px-3 py-1 text-[11px] font-semibold uppercase tracking-[0.14em] text-cyan-100">
                    Profile
                  </span>
                  <span class="inline-flex rounded-full border px-3 py-1 text-xs font-medium" [ngClass]="roleBadgeClasses()">
                    {{ primaryRole() }}
                  </span>
                  <span class="inline-flex items-center gap-2 rounded-full border px-3 py-1 text-xs font-medium" [ngClass]="availabilityBadgeClasses()">
                    <span class="h-2 w-2 rounded-full" [ngClass]="user().isActive && !user().isBanned ? 'bg-cyan-300' : 'bg-rose-300'"></span>
                    {{ availabilityLabel() }}
                  </span>
                </div>

                <div class="mt-4 min-w-0">
                  <div class="flex flex-wrap items-end gap-3">
                    <h2 class="truncate text-[2rem] font-semibold tracking-[-0.04em] sm:text-[2.3rem]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                      {{ user().userName || 'Unnamed user' }}
                    </h2>
                    <span class="pb-1 text-sm font-medium" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
                      {{ accountTypeLabel() }}
                    </span>
                  </div>
                  <p class="mt-2 truncate text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-300'">
                    {{ user().email }}
                  </p>
                  <div class="mt-3 flex flex-wrap items-center gap-2 text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
                    <span class="inline-flex items-center gap-1.5">
                      <span class="material-symbols-outlined text-sm">calendar_month</span>
                      Created {{ user().createdAt | date:'mediumDate' }}
                    </span>
                    <span class="inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/80 text-slate-600' : 'border-white/10 bg-white/[0.03] text-slate-300'">
                      <span class="material-symbols-outlined text-sm">verified</span>
                      {{ user().emailConfirmed ? 'Email verified' : 'Email pending' }}
                    </span>
                  </div>
                  @if (user().banReason) {
                    <div class="mt-4 rounded-[1.2rem] border px-3.5 py-3 text-sm" [ngClass]="theme() === 'light' ? 'border-rose-200 bg-rose-50/90 text-rose-700' : 'border-rose-400/20 bg-rose-500/10 text-rose-100'">
                      {{ user().banReason }}
                    </div>
                  }
                </div>
              </div>
            </div>
          </div>

          <div class="flex flex-wrap items-center gap-2 lg:max-w-[22rem] lg:justify-end">
            <button
              type="button"
              class="inline-flex min-h-11 items-center gap-2 rounded-full border px-4 py-2.5 text-sm font-medium transition"
              [ngClass]="theme() === 'light'
                ? 'cursor-not-allowed border-slate-200 bg-white/80 text-slate-400 opacity-70'
                : 'cursor-not-allowed border-white/10 bg-white/[0.04] text-slate-500 opacity-70'"
              disabled
              title="Connect an admin reset-password flow to enable this action."
            >
              <span class="material-symbols-outlined text-[18px]">lock_reset</span>
              Reset password
            </button>

            <button
              type="button"
              class="inline-flex min-h-11 items-center gap-2 rounded-full px-4 py-2.5 text-sm font-semibold transition active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
              [ngClass]="user().isBanned
                ? (theme() === 'light'
                    ? 'bg-emerald-500 text-white hover:bg-emerald-400'
                    : 'bg-cyan-400 text-slate-950 hover:bg-cyan-300')
                : (theme() === 'light'
                    ? 'bg-slate-950 text-white hover:bg-slate-800'
                    : 'bg-white text-slate-950 hover:bg-cyan-100')"
              [disabled]="!canToggleAccess() || accessActionLoading()"
              [attr.title]="canToggleAccess() ? '' : 'Your role cannot change user access.'"
              (click)="toggleAccess.emit()"
            >
              <span class="material-symbols-outlined text-[18px]">{{ user().isBanned ? 'refresh' : 'block' }}</span>
              {{ accessActionLoading() ? (user().isBanned ? 'Restoring...' : 'Disabling...') : (user().isBanned ? 'Restore access' : 'Disable user') }}
            </button>

            <div class="relative">
              <app-icon-button
                icon="more_horiz"
                variant="secondary"
                [theme]="theme()"
                title="More actions"
                ariaLabel="More actions"
                (click)="toggleMoreMenu()"
              />

              @if (showMoreMenu()) {
                <div
                  class="absolute right-0 top-[calc(100%+0.7rem)] z-20 min-w-[12rem] rounded-[1.2rem] border p-2 shadow-[0_24px_60px_-34px_rgba(15,23,42,0.7)] backdrop-blur-xl"
                  [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/95' : 'border-white/10 bg-slate-950/95'"
                >
                  <button
                    type="button"
                    class="flex w-full items-center gap-2 rounded-[0.9rem] px-3 py-2.5 text-left text-sm font-medium transition"
                    [ngClass]="theme() === 'light' ? 'text-slate-700 hover:bg-slate-100 hover:text-slate-950' : 'text-slate-200 hover:bg-white/[0.05] hover:text-white'"
                    (click)="handleCopyEmail()"
                  >
                    <span class="material-symbols-outlined text-[18px]">alternate_email</span>
                    Copy email
                  </button>
                  <button
                    type="button"
                    class="flex w-full items-center gap-2 rounded-[0.9rem] px-3 py-2.5 text-left text-sm font-medium transition"
                    [ngClass]="theme() === 'light' ? 'text-slate-700 hover:bg-slate-100 hover:text-slate-950' : 'text-slate-200 hover:bg-white/[0.05] hover:text-white'"
                    (click)="handleCopyId()"
                  >
                    <span class="material-symbols-outlined text-[18px]">content_copy</span>
                    Copy user ID
                  </button>
                </div>
              }
            </div>
          </div>
        </div>

        <div class="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
          <article class="rounded-[1.35rem] border px-4 py-4 transition hover:-translate-y-px" [ngClass]="statClasses()">
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-[0.14em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Roles</p>
                <p class="mt-3 text-3xl font-semibold tracking-[-0.04em]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                  {{ assignedRoleCount() }}
                </p>
              </div>
              <span class="inline-flex h-10 w-10 items-center justify-center rounded-2xl bg-cyan-400/12 text-cyan-100 ring-1 ring-cyan-400/15">
                <span class="material-symbols-outlined text-[20px]">shield_person</span>
              </span>
            </div>
          </article>

          <article class="rounded-[1.35rem] border px-4 py-4 transition hover:-translate-y-px" [ngClass]="statClasses()">
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-[0.14em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Linked apps</p>
                <p class="mt-3 text-3xl font-semibold tracking-[-0.04em]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                  {{ ssoAppCount() }}
                </p>
              </div>
              <span class="inline-flex h-10 w-10 items-center justify-center rounded-2xl bg-cyan-400/12 text-cyan-100 ring-1 ring-cyan-400/15">
                <span class="material-symbols-outlined text-[20px]">hub</span>
              </span>
            </div>
          </article>

          <article class="rounded-[1.35rem] border px-4 py-4 transition hover:-translate-y-px" [ngClass]="statClasses()">
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-[11px] font-semibold uppercase tracking-[0.14em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Last sign-in</p>
                <p class="mt-3 text-base font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-slate-100'">
                  Not tracked
                </p>
                <p class="mt-1 text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Telemetry not exposed by API</p>
              </div>
              <span class="inline-flex h-10 w-10 items-center justify-center rounded-2xl bg-cyan-400/12 text-cyan-100 ring-1 ring-cyan-400/15">
                <span class="material-symbols-outlined text-[20px]">schedule</span>
              </span>
            </div>
          </article>

          <article class="rounded-[1.35rem] border px-4 py-4 transition hover:-translate-y-px" [ngClass]="statClasses()">
            <div class="flex items-start justify-between gap-3">
              <div class="min-w-0">
                <p class="text-[11px] font-semibold uppercase tracking-[0.14em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">User ID</p>
                <p class="mt-3 truncate font-mono text-sm" [ngClass]="theme() === 'light' ? 'text-slate-800' : 'text-slate-100'">
                  {{ user().id }}
                </p>
              </div>
              <app-icon-button
                icon="content_copy"
                variant="ghost"
                [theme]="theme()"
                title="Copy user ID"
                ariaLabel="Copy user ID"
                (click)="copyId.emit()"
              />
            </div>
          </article>
        </div>
      </div>
    </section>
  `
})
export class UserAccessOverviewComponent {
  user = input.required<IdentityUser>();
  assignedRoleCount = input.required<number>();
  ssoAppCount = input.required<number>();
  accountTypeLabel = input.required<string>();
  availabilityLabel = input.required<string>();
  primaryRole = input.required<string>();
  canToggleAccess = input<boolean>(false);
  accessActionLoading = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');

  copyId = output<void>();
  copyEmail = output<void>();
  toggleAccess = output<void>();

  showMoreMenu = signal(false);

  handleCopyEmail() {
    this.copyEmail.emit();
    this.showMoreMenu.set(false);
  }

  handleCopyId() {
    this.copyId.emit();
    this.showMoreMenu.set(false);
  }

  toggleMoreMenu() {
    this.showMoreMenu.set(!this.showMoreMenu());
  }

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-[linear-gradient(180deg,rgba(255,255,255,0.96),rgba(240,249,255,0.88))]'
      : 'border-white/10 bg-[linear-gradient(180deg,rgba(15,23,42,0.9),rgba(2,8,23,0.92))]';
  }

  statClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/85 text-slate-900 shadow-[0_20px_45px_-38px_rgba(14,165,233,0.38)]'
      : 'border-white/10 bg-white/[0.04] text-slate-100 shadow-[inset_0_1px_0_rgba(255,255,255,0.05)]';
  }

  roleBadgeClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/90 text-slate-700'
      : 'border-white/10 bg-white/[0.05] text-slate-100';
  }

  availabilityBadgeClasses() {
    if (this.user().isActive && !this.user().isBanned) {
      return this.theme() === 'light'
        ? 'border-cyan-200 bg-cyan-50 text-cyan-700'
        : 'border-cyan-400/20 bg-cyan-400/12 text-cyan-100';
    }

    return this.theme() === 'light'
      ? 'border-rose-200 bg-rose-50 text-rose-700'
      : 'border-rose-400/20 bg-rose-500/12 text-rose-100';
  }
}
