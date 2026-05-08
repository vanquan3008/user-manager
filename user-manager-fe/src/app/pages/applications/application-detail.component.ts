import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApplicationsService } from '../../core/services/applications.service';
import { FileUploadService } from '../../core/services/file-upload.service';
import { RolesService } from '../../core/services/roles.service';
import { UsersService } from '../../core/services/users.service';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { DropdownComponent } from '../../shared/components/dropdown/dropdown.component';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import type { ApplicationListItem } from '../../core/services/applications.service';
import type { ApplicationRoleDto, RoleDto } from '../../core/models/api-response';
import type { IdentityUser } from '../../core/models/api-response';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    DrawerComponent,
    DrawerFooterActionsComponent,
    DropdownComponent,
    StatCardComponent,
    LoadingStateComponent,
    EmptyStateComponent
  ],
  templateUrl: './application-detail.component.html'
})
export class ApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly fileUploadService = inject(FileUploadService);
  private readonly rolesService = inject(RolesService);
  private readonly usersService = inject(UsersService);
  private readonly notify = inject(NotificationService);
  private readonly pageToolbar = inject(PageToolbarService);
  readonly theme = inject(ThemeService).theme;

  readonly app = signal<ApplicationListItem | null>(null);
  readonly appRoles = signal<ApplicationRoleDto[]>([]);
  readonly globalRoles = signal<RoleDto[]>([]);
  readonly appUsers = signal<Array<{ userId: string; role: string; email?: string; userName?: string }>>([]);
  readonly apiUsers = signal<IdentityUser[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly loadingRoles = signal(false);
  readonly loadingGlobalRoles = signal(false);
  readonly loadingUsers = signal(false);
  readonly showAddUserModal = signal(false);
  readonly closingAddUser = signal(false);
  readonly addUserUserId = signal('');
  readonly addUserRole = signal('');
  readonly savingAddUser = signal(false);
  readonly savingAddRole = signal(false);
  readonly showAddRoleModal = signal(false);
  readonly closingAddRole = signal(false);
  readonly updatingLogo = signal(false);
  readonly selectedGlobalRoleId = signal('');
  readonly defaultAppLogoDataUri = environment.defaultAppLogoDataUri;

  readonly totalUsers = computed(() => this.appUsers().length);
  readonly totalRoles = computed(() => this.appRoles().length);
  readonly userOptions = computed(() => [
    { value: '', label: '-- Select user --' },
    ...this.apiUsers().map(user => ({ value: user.id, label: this.getUserLabel(user) }))
  ]);
  readonly appRoleOptions = computed(() => [
    { value: '', label: '-- Select application role --' },
    ...this.appRoles().map(role => ({ value: role.name, label: role.name }))
  ]);
  readonly selectedApiUser = computed(() => this.apiUsers().find(user => user.id === this.addUserUserId()) ?? null);
  readonly globalRoleOptions = computed(() => [
    { value: '', label: '-- Select global role --' },
    ...this.globalRoles().map(role => ({ value: role.id, label: role.name }))
  ]);
  readonly selectedGlobalRole = computed(() => this.globalRoles().find(role => role.id === this.selectedGlobalRoleId()) ?? null);
  readonly canAddUsers = computed(() => this.appRoles().length > 0);

  ngOnInit() {
    const applicationId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!applicationId) {
      this.error.set('Application not found.');
      this.loading.set(false);
      return;
    }

    this.loadApplicationContext(applicationId);
  }

  loadApplicationContext(applicationId: string) {
    this.loading.set(true);
    this.error.set('');

    forkJoin({
      app: this.applicationsService.getApplication(applicationId),
      roles: this.applicationsService.getApplicationRoles(applicationId).pipe(catchError(() => of([] as ApplicationRoleDto[]))),
      users: this.applicationsService.getApplicationUsers(applicationId).pipe(catchError(() => of([] as Array<{ userId: string; role: string; email?: string; userName?: string }>)))
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ app, roles, users }) => {
          if (!app.success || !app.data) {
            this.error.set(app.message || 'Application not found.');
            return;
          }

          this.app.set(app.data);
          this.appRoles.set(roles);
          this.appUsers.set(users);
          const a = app.data;
          this.pageToolbar.setMeta({
            title: a.name,
            eyebrow: 'Application workspace',
            badge: 'Workspace',
            description:
              a.description?.trim() ||
              'Manage roles, branding and user access for this application workspace.'
          });
        },
        error: err => {
          this.error.set(err.error?.message || err.error?.error?.message || 'Failed to load application details.');
        }
      });
  }

  loadRoles(appId: string) {
    this.loadingRoles.set(true);
    this.applicationsService.getApplicationRoles(appId)
      .pipe(
        finalize(() => this.loadingRoles.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: roles => {
          this.appRoles.set(roles);
          if (this.showAddUserModal() && !this.addUserRole()) {
            this.addUserRole.set(this.getDefaultAppRoleName() ?? '');
          }
        },
        error: () => {
          this.notify.error('Failed to load application roles.');
        }
      });
  }

  loadAppUsers(appId: string) {
    this.applicationsService.getApplicationUsers(appId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: users => this.appUsers.set(users),
        error: () => this.notify.error('Failed to load application users.')
      });
  }

  loadApiUsers() {
    this.loadingUsers.set(true);
    this.usersService.getUsers(1, environment.usersPageSize)
      .pipe(
        finalize(() => this.loadingUsers.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          this.apiUsers.set(response.data || []);
        },
        error: () => {
          this.notify.error('Failed to load users.');
        }
      });
  }

  selectApiUser(userId: string) {
    this.addUserUserId.set(userId || '');
  }

  addUserToApp() {
    const app = this.app();
    const userId = this.addUserUserId().trim();
    const role = this.addUserRole().trim();

    if (!this.canAddUsers()) {
      this.notify.error('Create an application role before assigning users.');
      return;
    }

    if (!app || !userId || !role) {
      this.notify.error('Choose both a user and an application role.');
      return;
    }

    const roleMatched = this.appRoles().some(item => item.name.toLowerCase() === role.toLowerCase());
    if (!roleMatched) {
      this.notify.error('The selected role must belong to this application.');
      return;
    }

    this.savingAddUser.set(true);
    this.applicationsService.addUserToApplication(app.id, userId, role)
      .pipe(
        finalize(() => this.savingAddUser.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.message || 'Failed to add user.');
            return;
          }

          this.notify.success('User added to application.');
          this.addUserUserId.set('');
          this.closeAddUserModal();
          this.loadAppUsers(app.id);
        },
        error: err => {
          this.notify.error(err.error?.message || 'Failed to add user.');
        }
      });
  }

  quickAddUser() {
    this.openAddUserModal();
  }

  openAddUserModal() {
    if (!this.canAddUsers()) {
      this.notify.error('Create an application role first.');
      return;
    }

    this.addUserUserId.set('');
    this.addUserRole.set(this.getDefaultAppRoleName() ?? '');
    this.loadApiUsers();
    this.closingAddUser.set(false);
    this.showAddUserModal.set(true);
  }

  closeAddUserModal() {
    if (this.closingAddUser()) {
      return;
    }

    this.closingAddUser.set(true);
    setTimeout(() => {
      this.showAddUserModal.set(false);
      this.closingAddUser.set(false);
    }, 300);
  }

  getUserLabel(user: IdentityUser): string {
    return `${user.email || user.userName || 'Unknown user'} (${user.id})`;
  }

  private getGuestRoleName(): string | null {
    const guestRole = this.appRoles().find(role => role.name.toLowerCase() === 'guest');
    return guestRole?.name ?? null;
  }

  private getDefaultAppRoleName(): string | null {
    return this.getGuestRoleName() ?? this.appRoles()[0]?.name ?? null;
  }

  openAddRoleModal() {
    const app = this.app();
    if (app) {
      this.loadRoles(app.id);
    }

    this.loadGlobalRoles();
    this.selectedGlobalRoleId.set('');
    this.closingAddRole.set(false);
    this.showAddRoleModal.set(true);
  }

  loadGlobalRoles() {
    this.loadingGlobalRoles.set(true);
    this.rolesService.getRoles()
      .pipe(
        finalize(() => this.loadingGlobalRoles.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: roles => {
          this.globalRoles.set(roles);
        },
        error: () => {
          this.notify.error('Failed to load global roles.');
        }
      });
  }

  selectGlobalRole(roleId: string) {
    this.selectedGlobalRoleId.set(roleId || '');
  }

  closeAddRoleModal() {
    if (this.closingAddRole()) {
      return;
    }

    this.closingAddRole.set(true);
    setTimeout(() => {
      this.showAddRoleModal.set(false);
      this.closingAddRole.set(false);
    }, 300);
  }

  saveAddRole() {
    const app = this.app();
    const selectedRole = this.selectedGlobalRole();

    if (!app) {
      return;
    }

    if (!selectedRole) {
      this.notify.error('Select a global role.');
      return;
    }

    const roleName = selectedRole.name.trim();
    if (this.appRoles().some(role => role.name.toLowerCase() === roleName.toLowerCase())) {
      this.notify.error('This role already exists in the application.');
      return;
    }

    this.savingAddRole.set(true);
    this.applicationsService.createApplicationRole(app.id, {
      name: roleName,
      description: (selectedRole.description || '').trim() || null
    })
      .pipe(
        finalize(() => this.savingAddRole.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success) {
            this.notify.error(response.message || 'Failed to add role.');
            return;
          }

          this.notify.success(`Role '${roleName}' added.`);
          this.loadRoles(app.id);
          this.closeAddRoleModal();
        },
        error: err => {
          this.notify.error(err.error?.message || 'Failed to add role.');
        }
      });
  }

  onLogoFileSelected(event: Event) {
    const app = this.app();
    if (!app) {
      return;
    }

    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.notify.error('Please select a valid image file.');
      input.value = '';
      return;
    }

    const maxBytes = environment.maxImageUploadBytes;
    if (file.size > maxBytes) {
      this.notify.error('Image is too large. Please choose one under 2 MB.');
      input.value = '';
      return;
    }

    this.updatingLogo.set(true);
    this.fileUploadService.uploadImage(file, environment.uploadFolderAppLogos)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: uploadResponse => {
          const uploadedUrl = uploadResponse.data?.url;
          if (!uploadResponse.success || !uploadedUrl) {
            this.notify.error(uploadResponse.message || 'Logo upload failed.');
            this.updatingLogo.set(false);
            return;
          }

          this.applicationsService.updateApplicationLogo(app.id, uploadedUrl)
            .pipe(
              finalize(() => this.updatingLogo.set(false)),
              takeUntilDestroyed(this.destroyRef)
            )
            .subscribe({
              next: response => {
                if (response.success && response.data) {
                  this.app.set(response.data);
                  this.notify.success('Application logo updated.');
                } else {
                  this.notify.error(response.message || 'Failed to update logo.');
                }
              },
              error: err => {
                this.notify.error(err.error?.message || 'Failed to update logo.');
              }
            });
        },
        error: err => {
          this.notify.error(err.error?.message || 'Logo upload failed.');
          this.updatingLogo.set(false);
        }
      });
  }
}
