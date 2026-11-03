import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import type { SecuritySettings, SigningKeyInfo } from '../../core/models';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { SecurityService } from '../../core/services/security.service';
import { TranslateService } from '../../core/services/translate.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { formatDateTime } from '../../shared/utils/format';

@Component({
  selector: 'app-system',
  standalone: true,
  template: `
    <div class="page">
      <div class="page-header">
        <div><h1 class="page-title">{{ t().sysTitle }}</h1><p class="page-subtitle">{{ t().sysSubtitle }}</p></div>
      </div>

      @if (forbidden()) {
        <div class="alert-warn"><span class="material-symbols-outlined">lock</span><span>{{ t().sysNoPermission }}</span></div>
      } @else {
        <!-- Signing keys -->
        <section class="card">
          <div class="card-header">
            <div><h2 class="card-title">{{ t().sysKeysTitle }}</h2><p class="card-desc max-w-2xl">{{ t().sysKeysDesc }}</p></div>
            <button type="button" class="btn-primary btn-sm" (click)="rotate()" [disabled]="busy()"><span class="material-symbols-outlined">autorenew</span>{{ t().sysRotate }}</button>
          </div>
          <div class="table-wrap">
            <table class="table">
              <thead><tr><th>{{ t().sysKeyId }}</th><th>{{ t().sysAlgorithm }}</th><th>{{ t().sysStatus }}</th><th>{{ t().sysCreated }}</th><th>{{ t().sysPublishedUntil }}</th><th></th></tr></thead>
              <tbody>
                @for (k of keys(); track k.keyId) {
                  <tr>
                    <td class="font-mono text-[13px]">{{ k.keyId }}</td>
                    <td class="text-fg-2">{{ k.algorithm }} · {{ k.keySize }}-bit</td>
                    <td><span [class]="statusClass(k.status)"><span class="dot"></span>{{ statusLabel(k.status) }}</span></td>
                    <td class="whitespace-nowrap text-fg-2">{{ when(k.createdAt) }}</td>
                    <td class="whitespace-nowrap text-fg-2">{{ k.status === 'active' ? '—' : when(k.publishedUntil) }}</td>
                    <td class="text-right">
                      @if (k.status !== 'revoked') {
                        <button type="button" class="btn-danger-ghost btn-sm" (click)="revoke(k)" [disabled]="busy()">{{ t().sysRevoke }}</button>
                      }
                    </td>
                  </tr>
                } @empty {
                  <tr><td colspan="6" class="py-10 text-center text-fg-3">{{ t().loading }}</td></tr>
                }
              </tbody>
            </table>
          </div>
        </section>

        @if (settings(); as s) {
          <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <!-- Endpoints -->
            <section class="card lg:col-span-2">
              <div class="card-header"><div><h2 class="card-title">{{ t().sysEndpoints }}</h2><p class="card-desc">{{ t().sysEndpointsDesc }}</p></div></div>
              <div class="card-body grid gap-3 sm:grid-cols-2">
                @for (row of endpoints(); track row.label) {
                  <div class="min-w-0 rounded-lg border border-line px-3.5 py-3">
                    <p class="text-xs font-medium text-fg-3">{{ row.label }}</p>
                    <div class="mt-1 flex items-center gap-2">
                      <code class="min-w-0 flex-1 truncate font-mono text-[13px]">{{ row.value }}</code>
                      <button type="button" class="btn-icon-sm" (click)="copy(row.value)" [attr.aria-label]="t().sysCopy"><span class="material-symbols-outlined text-[16px]">content_copy</span></button>
                      @if (row.link) { <a [href]="row.value" target="_blank" rel="noopener" class="btn-icon-sm" aria-label="Open"><span class="material-symbols-outlined text-[16px]">open_in_new</span></a> }
                    </div>
                  </div>
                }
              </div>
            </section>

            @for (group of policy(); track group.title) {
              <section class="card">
                <div class="card-header"><h2 class="card-title">{{ group.title }}</h2></div>
                <dl class="divide-y divide-line">
                  @for (row of group.rows; track row.label) {
                    <div class="flex items-center justify-between gap-4 px-5 py-3 text-sm">
                      <dt class="text-fg-2">{{ row.label }}</dt><dd class="text-right font-medium tabular-nums">{{ row.value }}</dd>
                    </div>
                  }
                </dl>
              </section>
            }
          </div>
        }
      }
    </div>
  `
})
export class SystemComponent implements OnInit {
  private readonly security = inject(SecurityService);
  private readonly confirm = inject(ConfirmService);
  private readonly notify = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  readonly keys = signal<SigningKeyInfo[]>([]);
  readonly settings = signal<SecuritySettings | null>(null);
  readonly busy = signal(false);
  readonly forbidden = signal(false);

  readonly endpoints = computed(() => {
    const s = this.settings();
    if (!s) return [];
    const t = this.t();
    return [
      { label: t.sysIssuer, value: s.issuer, link: false },
      { label: t.sysAudience, value: s.audience, link: false },
      { label: t.sysDiscovery, value: s.discoveryUrl, link: true },
      { label: t.sysJwks, value: s.jwksUrl, link: true }
    ];
  });

  readonly policy = computed(() => {
    const s = this.settings();
    if (!s) return [];
    const t = this.t();
    const min = (n: number) => t.sysMinutes.replace('{{n}}', String(n));
    const days = (n: number) => (n > 0 ? t.sysDays.replace('{{n}}', String(n)) : t.sysDisabled);
    const sec = (n: number) => t.sysSeconds.replace('{{n}}', String(n));
    const rate = (n: number, w: number) => t.sysPerWindow.replace('{{n}}', String(n)).replace('{{w}}', String(w));
    const rules = [
      `≥ ${s.passwordMinLength}`,
      s.passwordRequiresUppercase ? 'A–Z' : null,
      s.passwordRequiresLowercase ? 'a–z' : null,
      s.passwordRequiresDigit ? '0–9' : null,
      s.passwordRequiresSymbol ? '#?!' : null
    ].filter(Boolean).join(' · ');
    return [
      {
        title: t.sysTokens,
        rows: [
          { label: t.sysAccessToken, value: min(s.accessTokenMinutes) },
          { label: t.sysRefreshIdle, value: days(s.refreshTokenIdleDays) },
          { label: t.sysSessionAbsolute, value: days(s.sessionAbsoluteDays) },
          { label: t.sysReuseGrace, value: sec(s.refreshReuseGraceSeconds) },
          { label: t.sysKeyRotation, value: days(s.signingKeyRotationDays) },
          { label: t.sysKeyRetention, value: days(s.retiredKeyRetentionDays) }
        ]
      },
      {
        title: t.sysProtection,
        rows: [
          { label: t.sysPasswordPolicy, value: rules },
          { label: t.sysLockout, value: t.sysLockoutValue.replace('{{n}}', String(s.lockoutMaxFailedAttempts)).replace('{{m}}', String(s.lockoutMinutes)) },
          { label: t.sysMfaChallenge, value: min(s.mfaChallengeMinutes) },
          { label: t.sysRateLimit, value: rate(s.rateLimitRequests, s.rateLimitWindowSeconds) },
          { label: t.sysAuthRateLimit, value: rate(s.authRateLimitRequests, s.authRateLimitWindowSeconds) },
          { label: t.sysAuditRetention, value: days(s.auditRetentionDays) }
        ]
      }
    ];
  });

  ngOnInit() { this.load(); }

  load() {
    this.security.signingKeys().subscribe({ next: k => this.keys.set(k), error: err => this.handle(err) });
    this.security.securitySettings().subscribe({ next: s => this.settings.set(s), error: err => this.handle(err) });
  }

  async rotate() {
    if (!(await this.confirm.confirm({ title: this.t().sysRotate, message: this.t().sysRotateConfirm, confirmText: this.t().sysRotate }))) return;
    this.busy.set(true);
    this.security.rotateSigningKey().subscribe({
      next: () => { this.busy.set(false); this.notify.success(this.t().sysRotated); this.load(); },
      error: err => { this.busy.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  async revoke(key: SigningKeyInfo) {
    if (!(await this.confirm.confirm({ title: `${this.t().sysRevoke} ${key.keyId}`, message: this.t().sysRevokeConfirm, confirmText: this.t().sysRevoke, variant: 'danger' }))) return;
    this.busy.set(true);
    this.security.revokeSigningKey(key.keyId).subscribe({
      next: () => { this.busy.set(false); this.notify.success(this.t().sysRevoked); this.load(); },
      error: err => { this.busy.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  async copy(value: string) {
    try { await navigator.clipboard.writeText(value); this.notify.success(this.t().accCopied); } catch { /* clipboard blocked */ }
  }

  statusClass(status: string) { return status === 'active' ? 'badge-ok' : status === 'revoked' ? 'badge-bad' : 'badge-neutral'; }
  statusLabel(status: string) { return status === 'active' ? this.t().sysActive : status === 'revoked' ? this.t().sysRevokedStatus : this.t().sysRetired; }
  when(iso: string | null | undefined) { return formatDateTime(iso, this.translate.locale()); }

  private handle(err: unknown) {
    if (err instanceof HttpErrorResponse && err.status === 403) this.forbidden.set(true);
    else this.notify.error(apiErrorMessage(err));
  }
}
