import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import type {
  ApplicationRoleDto,
  IdentityUser,
  RoleDto,
  SsoUserApplication,
  UserWithRoles
} from '../../core/models/api-response';
import { ApplicationsService, type ApplicationListItem } from '../../core/services/applications.service';
import { NotificationService } from '../../core/services/notification.service';
import { RolesService } from '../../core/services/roles.service';
import { ThemeService } from '../../core/services/theme.service';
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
  theme = inject(ThemeService).theme;

  userWithRoles = signal<UserWithRoles | null>(null);
  allRoles = signal<RoleDto[]>([]);
  loading = signal(true);
  error = signal('');
  actionMessage = signal('');
  selectedRole = signal('');
  addingRole = signal(false);
  removingRole = signal<string | null>(null);

  ssoApplications = signal<SsoUserApplication[]>([]);
  allApplications = signal<ApplicationListItem[]>([]);
  appRoles = signal<ApplicationRoleDto[]>([]);
  grantAppId = signal('');
  grantRole = signal('');
  granting = signal(false);
  revoking = signal<string | null>(null);

  assignedRoles = computed(() => this.userWithRoles()?.roles ?? []);
  userId = computed(() => this.userWithRoles()?.user?.id ?? '');
  isSsoUser = computed(() => this.userWithRoles()?.user?.isSsoAccount === true);
  accountTypeLabel = computed(() => (this.isSsoUser() ? 'SSO account' : 'Standard account'));
  availabilityLabel = computed(() => {
    const user = this.userWithRoles()?.user;
    return user?.isActive && !user?.isBanned ? 'Active' : 'Restricted';
  });

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

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Invalid user id');
      this.loading.set(false);
      return;
    }

    this.load(id);
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
        error: () => {
          this.appRoles.set([]);
        }
      });
  }

  addRole() {
    const role = this.selectedRole().trim();
    const uid = this.userId();

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
        error: err => {
          const message = getHttpErrorMessage(err, 'Failed to add role.', UNREACHABLE_SERVER_MESSAGE_VI);
          this.actionMessage.set(message);
          this.notify.error(message);
        }
      });
  }

  removeRole(roleName: string) {
    const uid = this.userId();
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
        error: err => {
          const message = getHttpErrorMessage(err, 'Failed to remove role.', UNREACHABLE_SERVER_MESSAGE_VI);
          this.actionMessage.set(message);
          this.notify.error(message);
        }
      });
  }

  grantSsoApplication() {
    const uid = this.userId();
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
        error: err => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to grant application access.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
  }

  revokeSsoApplication(appId: string) {
    const uid = this.userId();
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
        error: err => {
          this.notify.error(
            getHttpErrorMessage(err, 'Failed to revoke application access.', UNREACHABLE_SERVER_MESSAGE_VI)
          );
        }
      });
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
          this.pageToolbar.setMeta({
            title: (u as IdentityUser & { displayName?: string }).displayName || u.userName || u.email || 'User',
            eyebrow: 'User access',
            badge: u.isSsoAccount ? 'SSO account' : 'Standard account',
            description:
              'Review account status, manage global roles, and control SSO application access.'
          });

          if (user.data.user.isSsoAccount) {
            this.loadSsoApplications(userId);
          } else {
            this.ssoApplications.set([]);
            this.appRoles.set([]);
            this.grantAppId.set('');
            this.grantRole.set('');
          }
        },
        error: err => {
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
        error: () => {
          this.ssoApplications.set([]);
        }
      });
  }
}
