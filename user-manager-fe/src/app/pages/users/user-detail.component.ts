import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import type {
  ApplicationRoleDto,
  IdentityUser,
  ApplicationListItem,
  RoleDto,
  SsoUserApplication,
  UserWithRoles
} from '../../core/models';
import { ApplicationsService } from '../../core/services/applications.service';
import { NotificationService } from '../../core/services/notification.service';
import { RolesService } from '../../core/services/roles.service';
import { SecurityService } from '../../core/services/security.service';
import { TranslateService } from '../../core/services/translate.service';
import type { AuditLogEntry, MfaStatus, SessionInfo } from '../../core/models';
import { describeUserAgent, eventIcon, humanizeEvent, relativeTime } from '../../shared/utils/format';
import { UsersService } from '../../core/services/users.service';
import {
  UNREACHABLE_SERVER_MESSAGE_VI,
  getHttpErrorMessage
} from '../../core/utils/http-error.utils';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import type { DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import { UserAccessOverviewComponent } from './components/user-access-overview.component';
import { UserRoleManagementComponent } from './components/user-role-management.component';
import { UserSsoAccessComponent } from './components/user-sso-access.component';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { canBanUsers } from '../../core/constants/roles';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    LoadingStateComponent,
    UserAccessOverviewComponent,
    UserRoleManagementComponent,
    UserSsoAccessComponent
  ],
  templateUrl: './user-detail.component.html'
})
export class UserDetailComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private usersService = inject(UsersService);
  private applicationsService = inject(ApplicationsService);
  private rolesService = inject(RolesService);
  private notify = inject(NotificationService);
  private pageToolbar = inject(PageToolbarService);
  private auth = inject(AuthService);
  private confirm = inject(ConfirmService);
  private security = inject(SecurityService);
  private translate = inject(TranslateService);
  readonly t = this.translate.t;
  userId = input<string | null>(null);
  embedded = input(false);
  private lastLoadedUserId: string | null = null;

  userWithRoles = signal<UserWithRoles | null>(null);
  allRoles = signal<RoleDto[]>([]);
  loading = signal(true);
  error = signal('');
  actionMessage = signal('');
  selectedRole = signal('');
  addingRole = signal(false);
  removingRole = signal<string | null>(null);
  banActionLoading = signal(false);

  ssoApplications = signal<SsoUserApplication[]>([]);
  allApplications = signal<ApplicationListItem[]>([]);
  appRoles = signal<ApplicationRoleDto[]>([]);
  grantAppId = signal('');
  grantRole = signal('');
  granting = signal(false);
  revoking = signal<string | null>(null);

  // Security (MFA, sessions, activity)
  mfa = signal<MfaStatus | null>(null);
  sessions = signal<SessionInfo[]>([]);
  activity = signal<AuditLogEntry[]>([]);
  securityBusy = signal(false);

  assignedRoles = computed(() => this.userWithRoles()?.roles ?? []);
  resolvedUserId = computed(() => this.userWithRoles()?.user?.id ?? '');
  isSsoUser = computed(() => this.userWithRoles()?.user?.isSsoAccount === true);
  accountTypeLabel = computed(() => (this.isSsoUser() ? this.t().accSso : this.t().accConsole));
  availabilityLabel = computed(() => {
    const user = this.userWithRoles()?.user;
    return user?.isActive && !user?.isBanned ? this.t().usersActive : this.t().usersInactive;
  });
  canManageBan = computed(() => canBanUsers(this.auth.user()?.roles, this.auth.applications()));
  primaryRoleLabel = computed(() => this.sortRoleNames(this.assignedRoles())[0] ?? 'No role');

  applicationsAvailableToGrant = computed(() => {
    const grantedIds = new Set(this.ssoApplications().map(application => application.id));
    return this.allApplications().filter(application => !grantedIds.has(application.id));
  });

  rolesNotAssigned = computed(() => {
    const currentRoles = new Set(this.assignedRoles());
    return this.allRoles().filter(role => !currentRoles.has(role.name));
  });

  roleOptions = computed<DropdownOption[]>(() =>
    this.rolesNotAssigned().map(role => ({ value: role.name, label: role.name }))
  );

  applicationOptions = computed<DropdownOption[]>(() =>
    this.applicationsAvailableToGrant().map(application => ({
      value: application.id,
      label: application.name
    }))
  );

  appRoleOptions = computed<DropdownOption[]>(() =>
    this.appRoles().map(role => ({ value: role.name, label: role.name }))
  );

  constructor() {
    effect(() => {
      const embeddedUserId = this.userId();
      if (!embeddedUserId) {
        return;
      }

      this.tryLoad(embeddedUserId);
    });
  }

  ngOnInit() {
    if (this.userId()) {
      return;
    }

    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        if (!id) {
          this.error.set('Invalid user id');
          this.loading.set(false);
          return;
        }

        this.tryLoad(id);
      });
  }

  onGrantAppChange(appId: string) {
    this.grantAppId.set(appId);
    this.appRoles.set([]);
    this.grantRole.set('');

    if (!appId) {
      return;
    }

    this.applicationsService
      .getApplicationRoles(appId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: roles => {
          this.appRoles.set(roles);
          this.grantRole.set(roles[0]?.name ?? '');
        },
        error: (err: any) => {
          this.appRoles.set([]);
        }
      });
  }

  addRole() {
    const role = this.selectedRole().trim();
    const uid = this.resolvedUserId();

    if (!role || !uid || this.assignedRoles().includes(role)) {
      return;
    }

    this.addingRole.set(true);
    this.actionMessage.set('');

    this.usersService
      .addRole(uid, role)
      .pipe(
        finalize(() => this.addingRole.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success || !this.userWithRoles()) {
            this.notify.error(response.message || 'Failed to add role.');
            return;
          }

          const current = this.userWithRoles()!;
          this.userWithRoles.set({
            user: current.user,
            roles: [...current.roles, role]
          });
          this.selectedRole.set('');
          this.actionMessage.set(response.data?.message || `Added ${role}.`);
          this.notify.success(`Role ${role} added.`);
        },
        error: (err: any) => {
          const message = getHttpErrorMessage(err, 'Failed to add role.', UNREACHABLE_SERVER_MESSAGE_VI);
          this.actionMessage.set(message);
          this.notify.error(message);
        }
      });
  }

  removeRole(roleName: string) {
    const uid = this.resolvedUserId();
    if (!uid) return;

    this.removingRole.set(roleName);
    this.actionMessage.set('');

    this.usersService
      .removeRole(uid, roleName)
      .pipe(
        finalize(() => this.removingRole.set(null)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success || !this.userWithRoles()) {
            this.notify.error(response.message || 'Failed to remove role.');
            return;
          }

          const current = this.userWithRoles()!;
          this.userWithRoles.set({
            user: current.user,
            roles: current.roles.filter(role => role !== roleName)
          });
          this.actionMessage.set(response.message || `Removed ${roleName}.`);
          this.notify.success(`Role ${roleName} removed.`);
        },
        error: (err: any) => {
          const message = getHttpErrorMessage(err, 'Failed to remove role.', UNREACHABLE_SERVER_MESSAGE_VI);
          this.actionMessage.set(message);
          this.notify.error(message);
        }
      });
  }

  grantSsoApplication() {
    const uid = this.resolvedUserId();
    const appId = this.grantAppId().trim();
    const role = this.grantRole().trim();

    if (!uid || !appId || !role) {
      this.notify.error('Select both an application and a role.');
      return;
    }

    this.granting.set(true);
    this.usersService
      .grantSsoUserApplication(uid, appId, role)
      .pipe(
        finalize(() => this.granting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.message || 'Failed to grant application access.');
            return;
          }

          this.actionMessage.set(response.message || 'Application access granted.');
          this.notify.success('Application access granted.');
          this.grantAppId.set('');
          this.grantRole.set('');
          this.appRoles.set([]);
          this.loadSsoApplications(uid);
        },
        error: (err: any) => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to grant application access.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  revokeSsoApplication(appId: string) {
    const uid = this.resolvedUserId();
    if (!uid) return;

    this.revoking.set(appId);
    this.usersService
      .revokeSsoUserApplication(uid, appId)
      .pipe(
        finalize(() => this.revoking.set(null)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.message || 'Failed to revoke application access.');
            return;
          }

          this.actionMessage.set(response.message || 'Application access revoked.');
          this.notify.success('Application access revoked.');
          this.loadSsoApplications(uid);
        },
        error: (err: any) => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to revoke application access.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  async toggleUserAccess() {
    const current = this.userWithRoles()?.user;
    if (!current || !this.canManageBan() || this.banActionLoading()) {
      return;
    }

    if (current.isBanned) {
      this.unbanUser(current.id);
      return;
    }

    const accepted = await this.confirm.confirm({
      title: this.t().udDisable,
      message: this.t().udBanConfirm,
      confirmText: this.t().udDisable,
      variant: 'danger'
    });

    if (!accepted) {
      return;
    }

    this.banUser(current.id);
  }

  loadSecurity(userId: string) {
    this.security.userMfa(userId).subscribe({ next: m => this.mfa.set(m), error: () => this.mfa.set(null) });
    this.security.userSessions(userId).subscribe({ next: s => this.sessions.set(s), error: () => this.sessions.set([]) });
    this.security.auditLogs({ page: 1, pageSize: 8, userId }).subscribe({ next: r => this.activity.set(r.data ?? []), error: () => this.activity.set([]) });
  }

  async resetMfa() {
    const uid = this.resolvedUserId();
    if (!uid || !(await this.confirm.confirm({ title: this.t().udMfaReset, message: this.t().udMfaResetConfirm, confirmText: this.t().udMfaReset, variant: 'danger' }))) return;
    this.securityBusy.set(true);
    this.security.resetUserMfa(uid).subscribe({
      next: () => { this.securityBusy.set(false); this.notify.success(this.t().udMfaResetDone); this.loadSecurity(uid); this.patchUserState({ twoFactorEnabled: false }); },
      error: err => { this.securityBusy.set(false); this.notify.error(getHttpErrorMessage(err, this.t().authFailed, UNREACHABLE_SERVER_MESSAGE_VI)); }
    });
  }

  async revokeAllSessions() {
    const uid = this.resolvedUserId();
    if (!uid || !(await this.confirm.confirm({ title: this.t().udRevokeAll, message: this.t().udRevokeAllConfirm, confirmText: this.t().udRevokeAll, variant: 'danger' }))) return;
    this.securityBusy.set(true);
    this.security.revokeUserSessions(uid).subscribe({
      next: r => { this.securityBusy.set(false); this.notify.success(r.data?.message ?? this.t().udRevokeAll); this.loadSecurity(uid); },
      error: err => { this.securityBusy.set(false); this.notify.error(getHttpErrorMessage(err, this.t().authFailed, UNREACHABLE_SERVER_MESSAGE_VI)); }
    });
  }

  device(ua: string | null | undefined) { return describeUserAgent(ua); }
  ago(iso: string) { return relativeTime(iso, this.translate.locale()); }
  humanize = humanizeEvent;
  eventIcon = eventIcon;

  copyUserId() {
    this.copyText(this.userWithRoles()?.user?.id, 'User ID');
  }

  copyEmail() {
    this.copyText(this.userWithRoles()?.user?.email, 'Email');
  }

  private tryLoad(userId: string) {
    if (this.lastLoadedUserId === userId) {
      return;
    }

    this.lastLoadedUserId = userId;
    this.load(userId);
  }

  private load(userId: string) {
    this.loading.set(true);
    this.error.set('');

    forkJoin({
      roles: this.rolesService.getRoles().pipe(catchError(() => of([] as RoleDto[]))),
      applications: this.applicationsService.getApplications().pipe(
        catchError(() => of([] as ApplicationListItem[]))
      ),
      user: this.usersService.getUser(userId)
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ roles, applications, user }) => {
          this.allRoles.set(roles);
          this.allApplications.set(applications);

          if (!user.success || !user.data) {
            this.error.set('User not found');
            return;
          }

          this.userWithRoles.set(user.data);
          this.actionMessage.set('');

          const u = user.data.user;
          if (!this.embedded()) {
            this.pageToolbar.setMeta({
              title: (u as IdentityUser & { displayName?: string }).displayName || u.userName || u.email || 'User',
              eyebrow: 'User access',
              badge: u.isSsoAccount ? 'SSO account' : 'Standard account',
              description:
                'Review account status, manage global roles, and control SSO application access.'
            });
          }

          this.loadSecurity(userId);
          if (user.data.user.isSsoAccount) {
            this.loadSsoApplications(userId);
          } else {
            this.ssoApplications.set([]);
            this.appRoles.set([]);
            this.grantAppId.set('');
            this.grantRole.set('');
          }
        },
        error: (err: any) => {
          this.error.set(
            getHttpErrorMessage(err, 'Failed to load user details', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  private loadSsoApplications(userId: string) {
    this.usersService
      .getSsoUserApplications(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.ssoApplications.set(response.success && response.data ? response.data : []);
        },
        error: (err: any) => {
          this.ssoApplications.set([]);
        }
      });
  }

  private banUser(userId: string) {
    this.banActionLoading.set(true);
    this.usersService
      .banUser(userId)
      .pipe(
        finalize(() => this.banActionLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.error?.message || response.message || 'Failed to disable user.');
            return;
          }

          this.patchUserState({ isBanned: true });
          this.actionMessage.set(response.data?.message || response.message || 'User disabled.');
          this.notify.success(response.data?.message || 'User disabled.');
        },
        error: (err: any) => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to disable user.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  private unbanUser(userId: string) {
    this.banActionLoading.set(true);
    this.usersService
      .unbanUser(userId)
      .pipe(
        finalize(() => this.banActionLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.error?.message || response.message || 'Failed to restore access.');
            return;
          }

          this.patchUserState({ isBanned: false, banReason: null });
          this.actionMessage.set(response.data?.message || response.message || 'Access restored.');
          this.notify.success(response.data?.message || 'Access restored.');
        },
        error: (err: any) => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to restore access.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  private patchUserState(patch: Partial<IdentityUser>) {
    const current = this.userWithRoles();
    if (!current) {
      return;
    }

    this.userWithRoles.set({
      ...current,
      user: {
        ...current.user,
        ...patch
      }
    });
  }

  private async copyText(value: string | undefined | null, label: string) {
    if (!value) {
      this.notify.error(`${label} is not available.`);
      return;
    }

    try {
      await navigator.clipboard.writeText(value);
      this.notify.success(`${label} copied.`);
    } catch {
      this.notify.error(`Failed to copy ${label.toLowerCase()}.`);
    }
  }

  private sortRoleNames(roleNames: string[]) {
    const seen = new Set<string>();
    return [...roleNames]
      .filter(role => {
        const key = role.trim().toLowerCase();
        if (!key || seen.has(key)) {
          return false;
        }

        seen.add(key);
        return true;
      })
      .sort((left, right) => this.rolePriority(left) - this.rolePriority(right) || left.localeCompare(right));
  }

  private rolePriority(roleName: string) {
    const normalized = roleName.trim().toLowerCase();

    if (normalized.includes('superadmin')) return 0;
    if (normalized.includes('admin')) return 1;
    if (normalized.includes('owner')) return 2;
    if (normalized.includes('manager')) return 3;
    if (normalized.includes('editor')) return 4;
    if (normalized.includes('analyst')) return 5;
    if (normalized.includes('viewer')) return 6;
    if (normalized.includes('user')) return 7;

    return 8;
  }
}
