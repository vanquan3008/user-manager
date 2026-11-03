import { Component, inject, input, output } from '@angular/core';
import type { IdentityUser } from '../../../core/models';
import { TranslateService } from '../../../core/services/translate.service';
import { formatDateTime, initials, relativeTime } from '../../../shared/utils/format';

@Component({
  selector: 'app-user-access-overview',
  standalone: true,
  template: `
    <section class="card">
      <div class="flex flex-col gap-5 p-5 sm:flex-row sm:items-start">
        <span class="avatar h-14 w-14 text-lg">{{ initialsOf(user().userName || user().email) }}</span>
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-center gap-2">
            <h1 class="truncate text-xl font-semibold">{{ user().userName }}</h1>
            @if (user().isBanned) { <span class="badge-bad"><span class="dot"></span>{{ t().usersBanned }}</span> }
            @else if (user().isActive) { <span class="badge-ok"><span class="dot"></span>{{ availabilityLabel() }}</span> }
            @else { <span class="badge-neutral"><span class="dot"></span>{{ availabilityLabel() }}</span> }
            <span class="badge-neutral">{{ accountTypeLabel() }}</span>
            @if (user().twoFactorEnabled) { <span class="badge-brand"><span class="material-symbols-outlined text-[13px]">verified_user</span>MFA</span> }
          </div>
          <p class="mt-1 truncate text-sm text-fg-2">{{ user().email }}</p>
          @if (user().isBanned && user().banReason) {
            <p class="mt-2 text-[13px] text-bad">{{ t().udBanReason }}: {{ user().banReason }}</p>
          }
        </div>
        <div class="flex shrink-0 flex-wrap gap-2">
          <button type="button" class="btn-secondary btn-sm" (click)="copyEmail.emit()"><span class="material-symbols-outlined">mail</span>{{ t().udCopyEmail }}</button>
          <button type="button" class="btn-secondary btn-sm" (click)="copyId.emit()"><span class="material-symbols-outlined">fingerprint</span>{{ t().udCopyId }}</button>
          @if (canToggleAccess()) {
            <button type="button" [class]="user().isBanned ? 'btn-primary btn-sm' : 'btn-danger btn-sm'" (click)="toggleAccess.emit()" [disabled]="accessActionLoading()">
              @if (accessActionLoading()) { <span class="spinner"></span> }
              {{ user().isBanned ? t().udEnable : t().udDisable }}
            </button>
          }
        </div>
      </div>
      <dl class="grid grid-cols-2 gap-px border-t border-line bg-line sm:grid-cols-4">
        @for (s of stats(); track s.label) {
          <div class="bg-surface px-5 py-3.5">
            <dt class="text-xs text-fg-3">{{ s.label }}</dt>
            <dd class="mt-1 truncate text-sm font-medium">{{ s.value }}</dd>
          </div>
        }
      </dl>
    </section>
  `
})
export class UserAccessOverviewComponent {
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  user = input.required<IdentityUser>();
  assignedRoleCount = input.required<number>();
  ssoAppCount = input.required<number>();
  accountTypeLabel = input.required<string>();
  availabilityLabel = input.required<string>();
  primaryRole = input.required<string>();
  canToggleAccess = input<boolean>(false);
  accessActionLoading = input<boolean>(false);
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');

  copyId = output<void>();
  copyEmail = output<void>();
  toggleAccess = output<void>();

  initialsOf = initials;

  stats() {
    const u = this.user();
    const t = this.t();
    return [
      { label: t.accRoles, value: `${this.primaryRole()}${this.assignedRoleCount() > 1 ? ` +${this.assignedRoleCount() - 1}` : ''}` },
      { label: t.accEmailStatus, value: u.emailConfirmed ? t.accVerified : t.accUnverified },
      { label: t.usersColLastSignIn, value: u.lastLoginAt ? relativeTime(u.lastLoginAt, this.translate.locale()) : t.usersNever },
      { label: t.accMemberSince, value: formatDateTime(u.createdAt, this.translate.locale()) }
    ];
  }
}
