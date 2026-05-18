import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UsersService } from '../../core/services/users.service';
import { RolesService } from '../../core/services/roles.service';
import { ApplicationsService } from '../../core/services/applications.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { environment } from '../../../environments/environment';
import { canBanUsers } from '../../core/constants/roles';
import type { ApplicationListItem, IdentityUser } from '../../core/models';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

type DailyPoint = { date: string; label: string; count: number };

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCardComponent, LoadingStateComponent, EmptyStateComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly usersService = inject(UsersService);
  private readonly rolesService = inject(RolesService);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly notification = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService).theme;
  readonly translate = inject(TranslateService);
  private readonly pageToolbar = inject(PageToolbarService);

  readonly canBan = computed(() => canBanUsers(this.auth.user()?.roles, this.auth.applications()));
  readonly banActionId = signal<string | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly rolesCount = signal(0);
  readonly totalUsers = signal(0);
  readonly activeUsers = signal(0);
  readonly bannedUsers = signal(0);
  readonly ssoUsers = signal(0);
  readonly verifiedUsers = signal(0);
  readonly recentUsers = signal<IdentityUser[]>([]);
  readonly applications = signal<ApplicationListItem[]>([]);
  readonly registrations7d = signal<DailyPoint[]>([]);
  readonly appLogoFailed = signal<Set<string>>(new Set());

  readonly totalApplications = computed(() => this.applications().length);
  readonly activePercent = computed(() => this.toPercent(this.activeUsers(), this.totalUsers()));
  readonly bannedPercent = computed(() => this.toPercent(this.bannedUsers(), this.totalUsers()));
  readonly verifiedPercent = computed(() => this.toPercent(this.verifiedUsers(), this.totalUsers()));
  readonly ssoPercent = computed(() => this.toPercent(this.ssoUsers(), this.totalUsers()));
  readonly registrationMax = computed(() => Math.max(...this.registrations7d().map(item => item.count), 1));

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: this.translate.t().badge,
      description: this.translate.t().overviewDesc
    });
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');

    forkJoin({
      users: this.usersService.getUsers(1, environment.usersPageSize),
      roles: this.rolesService.getRoles().pipe(catchError(() => of([]))),
      applications: this.applicationsService.getApplications().pipe(catchError(() => of([])))
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ users, roles, applications }) => {
          const list = users.data || [];
          const recentUsers = [...list].sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime()).slice(0, 6);
          const activeUsers = list.filter(user => user.isActive && !user.isBanned).length;
          const bannedUsers = list.filter(user => user.isBanned).length;
          const ssoUsers = list.filter(user => user.isSsoAccount).length;
          const verifiedUsers = list.filter(user => user.emailConfirmed).length;

          this.totalUsers.set(users.totalCount ?? list.length);
          this.activeUsers.set(activeUsers);
          this.bannedUsers.set(bannedUsers);
          this.ssoUsers.set(ssoUsers);
          this.verifiedUsers.set(verifiedUsers);
          this.rolesCount.set(roles.length);
          this.recentUsers.set(recentUsers);
          this.applications.set(applications);
          this.registrations7d.set(this.buildRegistrationSeries(list));
          this.appLogoFailed.set(new Set());
        },
        error: (err: any) => {
          this.error.set(err.error?.message || err.error?.error?.message || 'Failed to load dashboard overview.');
        }
      });
  }

  async banUser(user: IdentityUser, event: Event): Promise<void> {
    event.preventDefault();
    event.stopPropagation();

    if (!this.canBan() || this.auth.user()?.id === user.id) {
      return;
    }

    const ok = await this.confirm.confirm({
      title: 'Confirm',
      message: this.translate.t().banUserConfirm,
      confirmText: 'OK',
      cancelText: this.translate.t().cancel || 'Cancel',
      variant: 'danger'
    });

    if (!ok) {
      return;
    }

    this.banActionId.set(user.id);
    this.usersService.banUser(user.id)
      .pipe(
        finalize(() => this.banActionId.set(null)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notification.error(response.error?.message || response.message || 'Error');
            return;
          }

          this.notification.success(response.data?.message ?? 'User banned.');
          this.load();
        },
        error: (err: any) => {
          const message = err?.error?.error?.message ?? err?.error?.message ?? 'Error';
          this.notification.error(typeof message === 'string' ? message : 'Error');
        }
      });
  }

  unbanUser(user: IdentityUser, event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (!this.canBan()) {
      return;
    }

    this.banActionId.set(user.id);
    this.usersService.unbanUser(user.id)
      .pipe(
        finalize(() => this.banActionId.set(null)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notification.error(response.error?.message || response.message || 'Error');
            return;
          }

          this.notification.success(response.data?.message ?? 'User unbanned.');
          this.load();
        },
        error: (err: any) => {
          const message = err?.error?.error?.message ?? err?.error?.message ?? 'Error';
          this.notification.error(typeof message === 'string' ? message : 'Error');
        }
      });
  }

  onAppLogoError(appId: string): void {
    this.appLogoFailed.update(current => new Set(current).add(appId));
  }

  registrationHeight(point: DailyPoint): number {
    return Math.max((point.count / this.registrationMax()) * 100, point.count > 0 ? 10 : 0);
  }

  private toPercent(value: number, total: number): number {
    if (!total) {
      return 0;
    }

    return Math.round((value / total) * 100);
  }

  private buildRegistrationSeries(users: IdentityUser[]): DailyPoint[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const buckets = new Map<string, DailyPoint>();
    for (let offset = 6; offset >= 0; offset--) {
      const current = new Date(today);
      current.setDate(today.getDate() - offset);
      const date = current.toISOString().slice(0, 10);
      buckets.set(date, {
        date,
        label: `${current.getDate()}/${current.getMonth() + 1}`,
        count: 0
      });
    }

    for (const user of users) {
      const createdDate = new Date(user.createdAt);
      createdDate.setHours(0, 0, 0, 0);
      const key = createdDate.toISOString().slice(0, 10);
      const bucket = buckets.get(key);
      if (bucket) {
        bucket.count += 1;
      }
    }

    return Array.from(buckets.values());
  }
}
