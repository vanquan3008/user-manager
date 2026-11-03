import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import type { DashboardSummary, IdentityUser } from '../../core/models';
import { SecurityService } from '../../core/services/security.service';
import { UsersService } from '../../core/services/users.service';
import { TranslateService } from '../../core/services/translate.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { StackedBarChartComponent, type StackedBarPoint } from '../../shared/components/charts/stacked-bar-chart.component';
import { eventIcon, humanizeEvent, initials, relativeTime } from '../../shared/utils/format';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, StatCardComponent, StackedBarChartComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1 class="page-title">{{ t().dashTitle }}</h1>
          <p class="page-subtitle">{{ t().dashSubtitle }}</p>
        </div>
        <button type="button" class="btn-secondary" (click)="load()" [disabled]="loading()">
          <span class="material-symbols-outlined" [class.animate-spin]="loading()">refresh</span>{{ t().dashRefresh }}
        </button>
      </div>

      @if (error()) {
        <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
      }

      <!-- KPI row -->
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        @if (summary(); as s) {
          <app-stat-card [label]="t().dashKpiUsers" [value]="s.users.total" icon="group" variant="primary" link="/users" [sublabel]="fill(t().dashKpiUsersSub, { n: s.users.newSince })" />
          <app-stat-card [label]="t().dashKpiSessions" [value]="s.activeSessions" icon="devices" [sublabel]="t().dashKpiSessionsSub" />
          <app-stat-card [label]="t().dashKpiMfa" [value]="s.mfaAdoptionRate + '%'" icon="phonelink_lock" link="/users" [variant]="s.mfaAdoptionRate >= 50 ? 'success' : 'warning'"
                         [sublabel]="fill(t().dashKpiMfaSub, { n: s.users.mfaEnabled, total: s.users.total })" />
          <app-stat-card [label]="t().dashKpiApps" [value]="s.applications" icon="apps" link="/applications" [sublabel]="t().dashKpiAppsSub" />
        } @else {
          @for (i of [0, 1, 2, 3]; track i) { <div class="card h-[118px] p-5"><div class="skeleton h-3 w-24"></div><div class="skeleton mt-6 h-7 w-16"></div></div> }
        }
      </div>

      <div class="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <!-- Sign-in activity -->
        <section class="card xl:col-span-2">
          <div class="card-header">
            <div><h2 class="card-title">{{ t().dashLoginActivity }}</h2><p class="card-desc">{{ t().dashLoginActivityDesc }}</p></div>
          </div>
          <div class="card-body">
            @if (summary()) {
              <app-stacked-bar-chart [points]="chartPoints()" [series]="chartSeries()" [ariaLabel]="t().dashLoginActivity"
                                     [chartLabel]="t().dashChart" [tableLabel]="t().dashTable" />
            } @else {
              <div class="skeleton h-64 w-full"></div>
            }
          </div>
        </section>

        <!-- Identity health -->
        <section class="card">
          <div class="card-header">
            <div><h2 class="card-title">{{ t().dashIdentityHealth }}</h2><p class="card-desc">{{ t().dashIdentityHealthDesc }}</p></div>
          </div>
          <div class="card-body space-y-4">
            @for (row of healthRows(); track row.label) {
              <div>
                <div class="mb-1.5 flex items-center justify-between text-[13px]">
                  <span class="inline-flex items-center gap-2 text-fg-2"><span class="material-symbols-outlined text-[17px] text-fg-3">{{ row.icon }}</span>{{ row.label }}</span>
                  <span class="tabular-nums"><span class="font-medium text-fg">{{ row.value }}</span><span class="ml-1.5 text-fg-3">{{ row.pct }}%</span></span>
                </div>
                <div class="h-2 overflow-hidden rounded-full bg-surface-3" role="meter" [attr.aria-valuenow]="row.pct" aria-valuemin="0" aria-valuemax="100" [attr.aria-label]="row.label">
                  <div class="h-full rounded-full transition-[width] duration-500" [class]="row.bar" [style.width.%]="row.pct"></div>
                </div>
              </div>
            }
            @if (summary() && summary()!.mfaAdoptionRate < 50) {
              <div class="alert-warn"><span class="material-symbols-outlined">shield</span><span>{{ t().dashMfaNudge }}</span></div>
            }
          </div>
        </section>
      </div>

      <div class="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <!-- Recent events -->
        <section class="card xl:col-span-2">
          <div class="card-header">
            <h2 class="card-title">{{ t().dashRecentEvents }}</h2>
            <a routerLink="/audit-logs" class="btn-ghost btn-sm">{{ t().dashViewAuditLog }}<span class="material-symbols-outlined">arrow_forward</span></a>
          </div>
          @if (summary()?.recentSecurityEvents?.length) {
            <ul class="divide-y divide-line">
              @for (e of summary()!.recentSecurityEvents; track e.id) {
                <li class="flex items-center gap-3 px-5 py-3">
                  <span class="icon-tile h-8 w-8" [class]="e.succeeded ? '' : '!bg-bad-soft !text-bad !ring-bad/20'"><span class="material-symbols-outlined text-[17px]">{{ icon(e.eventType) }}</span></span>
                  <div class="min-w-0 flex-1">
                    <p class="truncate text-sm font-medium">{{ humanize(e.eventType) }}</p>
                    <p class="truncate text-xs text-fg-3">{{ e.actorEmail || e.actorUserId || '—' }}@if (e.ipAddress) { · {{ e.ipAddress }} }</p>
                  </div>
                  <span [class]="e.succeeded ? 'badge-ok' : 'badge-bad'"><span class="dot"></span>{{ e.succeeded ? t().statusSucceeded : t().statusFailed }}</span>
                  <span class="hidden w-24 text-right text-xs text-fg-3 sm:block">{{ ago(e.occurredAt) }}</span>
                </li>
              }
            </ul>
          } @else if (summary()) {
            <p class="px-5 py-10 text-center text-sm text-fg-3">{{ t().dashNoEvents }}</p>
          } @else {
            <div class="space-y-3 p-5">@for (i of [0,1,2,3]; track i) { <div class="skeleton h-10 w-full"></div> }</div>
          }
        </section>

        <!-- Newest users -->
        <section class="card">
          <div class="card-header">
            <h2 class="card-title">{{ t().dashRecentUsers }}</h2>
            <a routerLink="/users" class="btn-ghost btn-sm">{{ t().viewAll }}<span class="material-symbols-outlined">arrow_forward</span></a>
          </div>
          <ul class="divide-y divide-line">
            @for (u of recentUsers(); track u.id) {
              <li>
                <a [routerLink]="['/users', u.id]" class="flex items-center gap-3 px-5 py-3 transition hover:bg-surface-2/60">
                  <span class="avatar">{{ initialsOf(u.userName || u.email) }}</span>
                  <div class="min-w-0 flex-1">
                    <p class="truncate text-sm font-medium">{{ u.userName }}</p>
                    <p class="truncate text-xs text-fg-3">{{ u.email }}</p>
                  </div>
                  @if (u.isBanned) { <span class="badge-bad">{{ t().dashBanned }}</span> }
                  @else if (u.twoFactorEnabled) { <span class="badge-ok" title="MFA"><span class="material-symbols-outlined text-[14px]">verified_user</span>MFA</span> }
                </a>
              </li>
            } @empty {
              <li class="px-5 py-10 text-center text-sm text-fg-3">{{ t().noUsers }}</li>
            }
          </ul>
        </section>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly security = inject(SecurityService);
  private readonly usersService = inject(UsersService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  readonly loading = signal(true);
  readonly error = signal('');
  readonly summary = signal<DashboardSummary | null>(null);
  readonly recentUsers = signal<IdentityUser[]>([]);

  readonly chartSeries = computed(() => [
    { name: this.t().dashSucceeded, color: 'var(--series-1)' },
    { name: this.t().dashFailed, color: 'var(--series-2)' }
  ]);

  readonly chartPoints = computed<StackedBarPoint[]>(() => {
    const locale = this.translate.locale();
    return (this.summary()?.loginActivity ?? []).map(p => {
      const d = new Date(p.day + 'T00:00:00');
      return {
        label: d.toLocaleDateString(locale, { day: 'numeric', month: 'short' }),
        detail: d.toLocaleDateString(locale, { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' }),
        values: [p.succeeded, p.failed]
      };
    });
  });

  readonly healthRows = computed(() => {
    const u = this.summary()?.users;
    const t = this.t();
    const pct = (v: number) => (u && u.total ? Math.round((v / u.total) * 100) : 0);
    if (!u) return [];
    return [
      { label: t.dashActive, value: u.active, pct: pct(u.active), icon: 'check_circle', bar: 'bg-ok' },
      { label: t.dashVerified, value: u.emailVerified, pct: pct(u.emailVerified), icon: 'mark_email_read', bar: 'bg-brand' },
      { label: t.dashSignedIn30, value: u.signedInSince, pct: pct(u.signedInSince), icon: 'login', bar: 'bg-info' },
      { label: t.dashLocked, value: u.locked, pct: pct(u.locked), icon: 'lock_clock', bar: 'bg-warn' },
      { label: t.dashBanned, value: u.banned, pct: pct(u.banned), icon: 'block', bar: 'bg-bad' }
    ];
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      summary: this.security.dashboard(),
      users: this.usersService.getUsers(1, 6).pipe(catchError(() => of(null)))
    })
      .pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ summary, users }) => {
          this.summary.set(summary);
          this.recentUsers.set((users?.data ?? []).slice(0, 6));
        },
        error: err => this.error.set(apiErrorMessage(err, this.t().dashLoadFailed))
      });
  }

  fill(template: string, values: Record<string, number>) {
    return Object.entries(values).reduce((acc, [k, v]) => acc.replace(`{{${k}}}`, String(v)), template);
  }

  ago(iso: string) { return relativeTime(iso, this.translate.locale()); }
  humanize = humanizeEvent;
  icon = eventIcon;
  initialsOf = initials;
}
