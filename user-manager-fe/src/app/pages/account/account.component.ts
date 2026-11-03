import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import QRCode from 'qrcode';
import type { AuditLogEntry, MfaSetup, MfaStatus, SessionInfo, UserWithRoles } from '../../core/models';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { SecurityService } from '../../core/services/security.service';
import { TranslateService } from '../../core/services/translate.service';
import { UsersService } from '../../core/services/users.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';
import { describeUserAgent, eventIcon, formatDateTime, humanizeEvent, initials, relativeTime } from '../../shared/utils/format';

type Tab = 'profile' | 'security' | 'sessions' | 'activity';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, TablePaginationComponent],
  templateUrl: './account.component.html'
})
export class AccountComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);
  private readonly users = inject(UsersService);
  private readonly security = inject(SecurityService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  readonly tab = signal<Tab>('profile');
  readonly tabs = computed(() => [
    { id: 'profile' as Tab, label: this.t().accTabProfile, icon: 'person' },
    { id: 'security' as Tab, label: this.t().accTabSecurity, icon: 'lock' },
    { id: 'sessions' as Tab, label: this.t().accTabSessions, icon: 'devices' },
    { id: 'activity' as Tab, label: this.t().accTabActivity, icon: 'history' }
  ]);

  // Profile
  readonly me = signal<UserWithRoles | null>(null);

  // Password
  readonly pwLoading = signal(false);
  readonly pwError = signal('');
  readonly pwForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: g => (g.get('newPassword')?.value === g.get('confirmPassword')?.value ? null : { mismatch: true }) }
  );

  // MFA
  readonly mfa = signal<MfaStatus | null>(null);
  readonly setup = signal<MfaSetup | null>(null);
  readonly qrDataUrl = signal('');
  readonly recoveryCodes = signal<string[] | null>(null);
  readonly mfaBusy = signal(false);
  readonly mfaError = signal('');
  readonly showDisable = signal(false);
  readonly showRegenerate = signal(false);
  enableCode = '';
  disablePassword = '';
  disableCode = '';
  regenerateCode = '';

  // Sessions & activity
  readonly sessions = signal<SessionInfo[]>([]);
  readonly sessionsLoading = signal(false);
  readonly activity = signal<AuditLogEntry[]>([]);
  readonly activityTotal = signal(0);
  readonly activityPage = signal(1);
  readonly activityPageSize = signal(25);
  readonly activityLoading = signal(false);

  readonly initials = computed(() => initials(this.me()?.user.userName || this.auth.user()?.email));

  ngOnInit() {
    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(q => {
      const tab = q.get('tab') as Tab | null;
      this.selectTab(tab && ['profile', 'security', 'sessions', 'activity'].includes(tab) ? tab : 'profile', false);
    });
    this.users.getMe().subscribe({ next: r => this.me.set(r.data ?? null) });
  }

  selectTab(tab: Tab, updateUrl = true) {
    this.tab.set(tab);
    if (updateUrl) this.router.navigate([], { queryParams: { tab }, replaceUrl: true });
    if (tab === 'security' && !this.mfa()) this.loadMfa();
    if (tab === 'sessions') this.loadSessions();
    if (tab === 'activity') this.loadActivity();
  }

  // ---- password
  changePassword() {
    if (this.pwForm.invalid || this.pwLoading()) {
      this.pwForm.markAllAsTouched();
      return;
    }
    this.pwLoading.set(true);
    this.pwError.set('');
    const { currentPassword, newPassword } = this.pwForm.getRawValue();
    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.pwLoading.set(false);
        this.pwForm.reset();
        this.notify.success(this.t().accPasswordChanged);
      },
      error: err => {
        this.pwLoading.set(false);
        this.pwError.set(apiErrorMessage(err));
      }
    });
  }

  // ---- MFA
  loadMfa() {
    this.security.mfaStatus().subscribe({ next: s => this.mfa.set(s), error: err => this.mfaError.set(apiErrorMessage(err)) });
  }

  beginSetup() {
    this.mfaBusy.set(true);
    this.mfaError.set('');
    this.security.beginMfaSetup().subscribe({
      next: async s => {
        this.setup.set(s);
        this.qrDataUrl.set(await QRCode.toDataURL(s.authenticatorUri, { margin: 1, width: 184, errorCorrectionLevel: 'M' }));
        this.mfaBusy.set(false);
      },
      error: err => { this.mfaBusy.set(false); this.mfaError.set(apiErrorMessage(err)); }
    });
  }

  cancelSetup() {
    this.setup.set(null);
    this.enableCode = '';
    this.mfaError.set('');
  }

  confirmEnable() {
    if (!/^\d{6}$/.test(this.enableCode.trim())) return;
    this.mfaBusy.set(true);
    this.mfaError.set('');
    this.security.enableMfa(this.enableCode.trim()).subscribe({
      next: codes => {
        this.mfaBusy.set(false);
        this.setup.set(null);
        this.enableCode = '';
        this.recoveryCodes.set(codes);
        this.notify.success(this.t().accMfaEnabled);
        this.loadMfa();
      },
      error: err => { this.mfaBusy.set(false); this.mfaError.set(apiErrorMessage(err)); }
    });
  }

  disable() {
    if (!this.disablePassword || !this.disableCode.trim()) return;
    this.mfaBusy.set(true);
    this.mfaError.set('');
    this.security.disableMfa(this.disablePassword, this.disableCode.trim()).subscribe({
      next: () => {
        this.mfaBusy.set(false);
        this.showDisable.set(false);
        this.disablePassword = this.disableCode = '';
        this.notify.success(this.t().accMfaDisabled);
        this.loadMfa();
      },
      error: err => { this.mfaBusy.set(false); this.mfaError.set(apiErrorMessage(err)); }
    });
  }

  regenerate() {
    if (!/^\d{6}$/.test(this.regenerateCode.trim())) return;
    this.mfaBusy.set(true);
    this.mfaError.set('');
    this.security.regenerateRecoveryCodes(this.regenerateCode.trim()).subscribe({
      next: codes => {
        this.mfaBusy.set(false);
        this.showRegenerate.set(false);
        this.regenerateCode = '';
        this.recoveryCodes.set(codes);
        this.loadMfa();
      },
      error: err => { this.mfaBusy.set(false); this.mfaError.set(apiErrorMessage(err)); }
    });
  }

  async copyCodes() {
    try {
      await navigator.clipboard.writeText((this.recoveryCodes() ?? []).join('\n'));
      this.notify.success(this.t().accCopied);
    } catch {
      this.notify.error(this.t().authFailed);
    }
  }

  downloadCodes() {
    const blob = new Blob([`${this.t().productName} recovery codes\n\n${(this.recoveryCodes() ?? []).join('\n')}\n`], { type: 'text/plain' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'recovery-codes.txt';
    a.click();
    URL.revokeObjectURL(a.href);
  }

  // ---- sessions
  loadSessions() {
    this.sessionsLoading.set(true);
    this.security.mySessions().subscribe({
      next: s => { this.sessions.set(s); this.sessionsLoading.set(false); },
      error: err => { this.sessionsLoading.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  async revoke(session: SessionInfo) {
    if (session.isCurrent) {
      this.auth.logout();
      return;
    }
    if (!(await this.confirm.confirm({ title: this.t().accRevoke, message: this.t().accRevokeConfirm, confirmText: this.t().accRevoke, variant: 'danger' }))) return;
    this.security.revokeMySession(session.id).subscribe({
      next: () => this.loadSessions(),
      error: err => this.notify.error(apiErrorMessage(err))
    });
  }

  async revokeOthers() {
    if (!(await this.confirm.confirm({ title: this.t().accSignOutOthers, message: this.t().accSignOutOthersConfirm, confirmText: this.t().accSignOutOthers, variant: 'danger' }))) return;
    this.security.revokeOtherSessions().subscribe({
      next: r => { this.notify.success(r.data?.message ?? this.t().accSignOutOthers); this.loadSessions(); },
      error: err => this.notify.error(apiErrorMessage(err))
    });
  }

  // ---- activity
  loadActivity() {
    this.activityLoading.set(true);
    this.security.myActivity(this.activityPage(), this.activityPageSize()).subscribe({
      next: r => { this.activity.set(r.data ?? []); this.activityTotal.set(r.totalCount ?? 0); this.activityLoading.set(false); },
      error: err => { this.activityLoading.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  onActivityPage(page: number) { this.activityPage.set(page); this.loadActivity(); }
  onActivityPageSize(size: number) { this.activityPageSize.set(size); this.activityPage.set(1); this.loadActivity(); }

  recoveryRemaining() { return this.t().accRecoveryRemaining.replace('{{n}}', String(this.mfa()?.recoveryCodesRemaining ?? 0)); }
  device(ua: string | null | undefined) { return describeUserAgent(ua); }
  ago(iso: string) { return relativeTime(iso, this.translate.locale()); }
  when(iso: string | null | undefined) { return formatDateTime(iso, this.translate.locale()); }
  humanize = humanizeEvent;
  icon = eventIcon;
}
