import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { ClockService } from '../../core/services/clock.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime } from 'rxjs';
import type { AuditLogEntry, AuditLogFilter } from '../../core/models';
import { NotificationService } from '../../core/services/notification.service';
import { SecurityService } from '../../core/services/security.service';
import { TranslateService } from '../../core/services/translate.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';
import { describeUserAgent, eventIcon, formatDateTime, humanizeEvent, relativeTime } from '../../shared/utils/format';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [FormsModule, TablePaginationComponent, DrawerComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <div><h1 class="page-title">{{ t().auditTitle }}</h1><p class="page-subtitle">{{ t().auditSubtitle }}</p></div>
        <button type="button" class="btn-secondary" (click)="export()" [disabled]="exporting()">
          @if (exporting()) { <span class="spinner"></span> } @else { <span class="material-symbols-outlined">download</span> }{{ t().auditExport }}
        </button>
      </div>

      <section class="card">
        <!-- Filters: one row above the data -->
        <div class="flex flex-wrap items-end gap-3 border-b border-line p-4">
          <div class="input-group min-w-[220px] flex-1">
            <span class="material-symbols-outlined">search</span>
            <input class="input" type="search" [placeholder]="t().auditSearch" [ngModel]="filter().search" (ngModelChange)="onSearch($event)" [attr.aria-label]="t().auditSearch" />
          </div>
          <select class="input w-auto min-w-[160px]" [ngModel]="filter().category ?? ''" (ngModelChange)="patch({ category: $event || undefined })" [attr.aria-label]="t().auditAllCategories">
            <option value="">{{ t().auditAllCategories }}</option>
            @for (c of categories(); track c.value) { <option [value]="c.value">{{ c.label }}</option> }
          </select>
          <select class="input w-auto min-w-[140px]" [ngModel]="resultValue()" (ngModelChange)="patch({ succeeded: $event === '' ? null : $event === 'true' })" [attr.aria-label]="t().auditAllResults">
            <option value="">{{ t().auditAllResults }}</option>
            <option value="true">{{ t().auditOnlySucceeded }}</option>
            <option value="false">{{ t().auditOnlyFailed }}</option>
          </select>
          <label class="flex items-center gap-2 text-[13px] text-fg-2">{{ t().auditFrom }}
            <input type="date" class="input w-auto" [ngModel]="filter().from" (ngModelChange)="patch({ from: $event || undefined })" />
          </label>
          <label class="flex items-center gap-2 text-[13px] text-fg-2">{{ t().auditTo }}
            <input type="date" class="input w-auto" [ngModel]="filter().to" (ngModelChange)="patch({ to: $event || undefined })" />
          </label>
          @if (hasFilters()) { <button type="button" class="btn-ghost btn-sm" (click)="clear()">{{ t().auditClear }}</button> }
        </div>

        <div class="table-wrap">
          <table class="table table-clickable">
            <thead><tr><th>{{ t().colEvent }}</th><th>{{ t().colResult }}</th><th>{{ t().colActor }}</th><th>{{ t().colTarget }}</th><th>{{ t().colIp }}</th><th class="text-right">{{ t().colWhen }}</th></tr></thead>
            <tbody>
              @for (e of items(); track e.id) {
                <tr (click)="selected.set(e)" tabindex="0" (keydown.enter)="selected.set(e)">
                  <td>
                    <span class="inline-flex items-center gap-2.5">
                      <span class="material-symbols-outlined text-[18px]" [class]="e.succeeded ? 'text-fg-3' : 'text-bad'">{{ icon(e.eventType) }}</span>
                      <span><span class="block font-medium">{{ humanize(e.eventType) }}</span><span class="block font-mono text-2xs text-fg-3">{{ e.eventType }}</span></span>
                    </span>
                  </td>
                  <td><span [class]="e.succeeded ? 'badge-ok' : 'badge-bad'"><span class="dot"></span>{{ e.succeeded ? t().statusSucceeded : t().statusFailed }}</span></td>
                  <td class="max-w-[220px] truncate">{{ e.actorEmail || e.actorUserId || '—' }}</td>
                  <td class="max-w-[180px] truncate text-fg-2">{{ e.targetType ? e.targetType + ' · ' + (e.targetId ?? '') : '—' }}</td>
                  <td class="font-mono text-xs text-fg-2">{{ e.ipAddress || '—' }}</td>
                  <td class="whitespace-nowrap text-right text-fg-2" [title]="when(e.occurredAt)">{{ ago(e.occurredAt) }}</td>
                </tr>
              } @empty {
                <tr><td colspan="6" class="py-14 text-center text-fg-3">{{ loading() ? t().loading : t().auditEmpty }}</td></tr>
              }
            </tbody>
          </table>
        </div>
        <app-table-pagination [totalItems]="total()" [page]="filter().page" [pageSize]="filter().pageSize"
                              (pageChange)="patch({ page: $event }, false)" (pageSizeChange)="patch({ pageSize: $event })" />
      </section>
    </div>

    @if (selected(); as e) {
      <app-drawer [title]="humanize(e.eventType)" [subtitle]="t().auditDetail" (close)="selected.set(null)">
        <div class="space-y-5">
          <span [class]="e.succeeded ? 'badge-ok' : 'badge-bad'"><span class="dot"></span>{{ e.succeeded ? t().statusSucceeded : t().statusFailed }}</span>
          <dl class="dl sm:grid-cols-1">
            <div><dt>{{ t().colEvent }}</dt><dd class="font-mono text-[13px]">{{ e.eventType }}</dd></div>
            <div><dt>{{ t().colWhen }}</dt><dd>{{ when(e.occurredAt) }}</dd></div>
            <div><dt>{{ t().colActor }}</dt><dd>{{ e.actorEmail || '—' }} @if (e.actorUserId) { <span class="block font-mono text-xs text-fg-3">{{ e.actorUserId }}</span> }</dd></div>
            @if (e.targetType) { <div><dt>{{ t().colTarget }}</dt><dd>{{ e.targetType }} <span class="block font-mono text-xs text-fg-3">{{ e.targetId }}</span></dd></div> }
            @if (e.reason) { <div><dt>{{ t().auditReason }}</dt><dd>{{ e.reason }}</dd></div> }
            <div><dt>{{ t().colIp }}</dt><dd class="font-mono text-[13px]">{{ e.ipAddress || '—' }}</dd></div>
            <div><dt>{{ t().colDevice }}</dt><dd>{{ device(e.userAgent) }}<span class="mt-1 block break-all text-xs text-fg-3">{{ e.userAgent }}</span></dd></div>
            @if (e.sessionId) { <div><dt>{{ t().auditSession }}</dt><dd class="font-mono text-xs">{{ e.sessionId }}</dd></div> }
            @if (e.applicationId) { <div><dt>{{ t().auditApplication }}</dt><dd class="font-mono text-xs">{{ e.applicationId }}</dd></div> }
            @if (e.correlationId) { <div><dt>{{ t().auditCorrelation }}</dt><dd class="font-mono text-xs">{{ e.correlationId }}</dd></div> }
          </dl>
          @if (e.details) {
            <div>
              <p class="mb-1.5 text-xs font-medium text-fg-3">{{ t().auditDetailsJson }}</p>
              <pre class="overflow-x-auto rounded-lg bg-surface-2 p-3 font-mono text-xs text-fg">{{ pretty(e.details) }}</pre>
            </div>
          }
        </div>
      </app-drawer>
    }
  `
})
export class AuditLogsComponent implements OnInit {
  private readonly clock = inject(ClockService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly security = inject(SecurityService);
  private readonly notify = inject(NotificationService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  readonly filter = signal<AuditLogFilter>({ page: 1, pageSize: 25 });
  readonly items = signal<AuditLogEntry[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly exporting = signal(false);
  readonly selected = signal<AuditLogEntry | null>(null);
  private readonly search$ = new Subject<string>();

  readonly categories = computed(() => {
    const t = this.t();
    return [
      { value: 'auth', label: t.auditCatAuth }, { value: 'session', label: t.auditCatSession },
      { value: 'mfa', label: t.auditCatMfa }, { value: 'oauth', label: t.auditCatOauth },
      { value: 'user', label: t.auditCatUser }, { value: 'application', label: t.auditCatApplication },
      { value: 'security', label: t.auditCatSecurity }
    ];
  });
  readonly resultValue = computed(() => (this.filter().succeeded === null || this.filter().succeeded === undefined ? '' : String(this.filter().succeeded)));
  readonly hasFilters = computed(() => {
    const f = this.filter();
    return !!(f.search || f.category || f.from || f.to || (f.succeeded !== null && f.succeeded !== undefined));
  });

  ngOnInit() {
    this.search$.pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef)).subscribe(search => this.patch({ search }));
    this.load();
  }

  onSearch(value: string) { this.search$.next(value); }

  patch(change: Partial<AuditLogFilter>, resetPage = true) {
    this.filter.update(f => ({ ...f, ...change, page: resetPage && change.page === undefined ? 1 : change.page ?? f.page }));
    this.load();
  }

  clear() {
    this.filter.set({ page: 1, pageSize: this.filter().pageSize });
    this.load();
  }

  load() {
    this.loading.set(true);
    this.security.auditLogs(this.filter()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: r => { this.items.set(r.data ?? []); this.total.set(r.totalCount ?? 0); this.loading.set(false); },
      error: err => { this.loading.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  export() {
    this.exporting.set(true);
    this.security.exportAuditLogs(this.filter()).subscribe({
      next: blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = `audit-log-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(a.href);
        this.exporting.set(false);
      },
      error: err => { this.exporting.set(false); this.notify.error(apiErrorMessage(err)); }
    });
  }

  pretty(json: string) { try { return JSON.stringify(JSON.parse(json), null, 2); } catch { return json; } }
  device(ua: string | null | undefined) { return describeUserAgent(ua).label; }
  ago(iso: string) { return relativeTime(iso, this.translate.locale(), this.clock.now()); }
  when(iso: string) { return formatDateTime(iso, this.translate.locale()); }
  humanize = humanizeEvent;
  icon = eventIcon;
}
