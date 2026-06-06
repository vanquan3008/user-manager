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
import { ConfirmService } from '../../core/services/confirm.service';
import { environment } from '../../../environments/environment';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { DropdownComponent } from '../../shared/components/dropdown/dropdown.component';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import type {
  ApplicationCapabilitiesDto,
  ApplicationCredentialItem,
  ApplicationListItem,
  ApplicationRoleDto,
  ApplicationUserDto,
  IdentityUser,
  RoleDto
} from '../../core/models';

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
  private readonly confirm = inject(ConfirmService);
  private readonly pageToolbar = inject(PageToolbarService);
  readonly theme = inject(ThemeService).theme;

  readonly app = signal<ApplicationListItem | null>(null);
  readonly credentials = signal<ApplicationCredentialItem | null>(null);
  readonly capabilities = signal<ApplicationCapabilitiesDto | null>(null);
  readonly appRoles = signal<ApplicationRoleDto[]>([]);
  readonly globalRoles = signal<RoleDto[]>([]);
  readonly appUsers = signal<ApplicationUserDto[]>([]);
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
  readonly rotatingSecret = signal(false);
  readonly showClientSecret = signal(false);
  readonly selectedGlobalRoleId = signal('');
  readonly defaultAppLogoDataUri = environment.defaultAppLogoDataUri;

  readonly totalUsers = computed(() => this.appUsers().length);
  readonly totalRoles = computed(() => this.appRoles().length);
  readonly ownerLabel = computed(() => this.app()?.ownerDisplayName?.trim() || this.app()?.ownerId?.trim() || 'Unassigned');
  readonly clientTypeLabel = computed(() => this.credentials()?.clientType || this.app()?.clientType || 'Public');
  readonly redirectUris = computed(() =>
    this.parseDelimitedValues(this.credentials()?.allowedRedirectUris || this.app()?.allowedRedirectUris || '')
  );
  readonly redirectHosts = computed(() =>
    Array.from(
      new Set(
        this.redirectUris().map((uri) => {
          try {
            return new URL(uri).host;
          } catch {
            return uri;
          }
        })
      )
    )
  );
  readonly primaryRedirectHost = computed(() => this.redirectHosts()[0] || 'No redirect host');
  readonly capabilityFeatureCount = computed(() => this.capabilities()?.enabledFeatures.length ?? 0);
  readonly capabilityPermissionCount = computed(() => this.capabilities()?.enabledPermissions.length ?? 0);
  readonly visibleClientSecret = computed(() => !!this.credentials()?.clientSecret?.trim());
  readonly hasBackchannelLogout = computed(() => !!(this.credentials()?.backchannelLogoutUri || this.app()?.backchannelLogoutUri || '').trim());
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
      credentials: this.applicationsService.getApplicationCredentials(applicationId).pipe(catchError(() => of(null))),
      capabilities: this.applicationsService.getApplicationCapabilities(applicationId).pipe(catchError(() => of(null))),
      roles: this.applicationsService.getApplicationRoles(applicationId).pipe(catchError(() => of([] as ApplicationRoleDto[]))),
      users: this.applicationsService.getApplicationUsers(applicationId).pipe(catchError(() => of([] as ApplicationUserDto[])))
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: ({ app, credentials, capabilities, roles, users }) => {
          if (!app.success || !app.data) {
            this.error.set(app.message || 'Application not found.');
            return;
          }

          this.showClientSecret.set(false);
          this.app.set(app.data);
          this.credentials.set(credentials?.success ? credentials.data : null);
          this.capabilities.set(capabilities);
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
        error: (err: any) => {
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
        error: (err: any) => {
          this.notify.error('Failed to load application roles.');
        }
      });
  }

  loadAppUsers(appId: string) {
    this.applicationsService.getApplicationUsers(appId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: users => this.appUsers.set(users),
        error: (err: any) => this.notify.error('Failed to load application users.')
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
        error: (err: any) => {
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
        error: (err: any) => {
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

  getUserDisplayName(user: ApplicationUserDto): string {
    return user.email || user.userName || user.userId;
  }

  getUserInitial(user: ApplicationUserDto): string {
    return this.getUserDisplayName(user).charAt(0).toUpperCase();
  }

  getAppInitials(): string {
    const name = this.app()?.name?.trim() || '';
    if (!name) return 'AP';

    const parts = name
      .split(/[\s-_]+/)
      .map((part) => part.trim())
      .filter(Boolean);

    if (parts.length === 1) {
      return parts[0].slice(0, 2).toUpperCase();
    }

    return `${parts[0][0] ?? ''}${parts[1][0] ?? ''}`.toUpperCase();
  }

  getSecretStatusLabel(): string {
    if (this.clientTypeLabel().toLowerCase() !== 'confidential') {
      return 'PKCE only';
    }

    if (this.visibleClientSecret()) {
      return 'New secret ready';
    }

    if (this.credentials()?.lastClientSecretRotatedAt) {
      return 'Managed secret';
    }

    return 'Needs rotation';
  }

  getSecretStatusDescription(): string {
    if (this.clientTypeLabel().toLowerCase() !== 'confidential') {
      return 'Public clients authenticate with PKCE and do not store a client secret.';
    }

    if (this.visibleClientSecret()) {
      return 'This one-time value is visible now. Copy it before leaving the page.';
    }

    if (this.credentials()?.lastClientSecretRotatedAt) {
      return 'The secret exists server-side, but its raw value is no longer readable from this screen.';
    }

    return 'Rotate once to reveal a fresh secret value for integrations.';
  }

  displayClientSecret(): string {
    const value = this.credentials()?.clientSecret?.trim() || '';
    if (!value) {
      return 'Stored server-side only. Rotate to issue a fresh one-time value.';
    }

    if (this.showClientSecret()) {
      return value;
    }

    return `${value.slice(0, 4)}${'•'.repeat(Math.max(12, value.length - 8))}${value.slice(-4)}`;
  }

  toggleClientSecretVisibility() {
    if (!this.visibleClientSecret()) {
      return;
    }

    this.showClientSecret.update((value) => !value);
  }

  private parseDelimitedValues(raw: string): string[] {
    if (!raw.trim()) {
      return [];
    }

    return Array.from(
      new Set(
        raw
          .split(/[\r\n,]+/)
          .map(value => value.trim())
          .filter(Boolean)
      )
    );
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
        error: (err: any) => {
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
        error: (err: any) => {
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
              error: (err: any) => {
                this.notify.error(err.error?.message || 'Failed to update logo.');
              }
            });
        },
        error: (err: any) => {
          this.notify.error(err.error?.message || 'Logo upload failed.');
          this.updatingLogo.set(false);
        }
      });
  }

  async rotateClientSecret() {
    const app = this.app();
    const credentials = this.credentials();
    if (!app || !credentials || credentials.clientType !== 'Confidential') {
      this.notify.error('This application does not use a confidential client secret.');
      return;
    }

    const confirmed = await this.confirm.confirm({
      title: 'Rotate client secret',
      message: 'Rotate the client secret now? The previous secret will stop working after this action.',
      confirmText: 'Rotate',
      cancelText: 'Cancel',
      variant: 'danger'
    });

    if (!confirmed) {
      return;
    }

    this.rotatingSecret.set(true);
    this.applicationsService.rotateApplicationClientSecret(app.id)
      .pipe(
        finalize(() => this.rotatingSecret.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          if (!response.success || !response.data) {
            this.notify.error(response.message || 'Failed to rotate client secret.');
            return;
          }

          this.credentials.set(response.data);
          this.showClientSecret.set(false);
          this.notify.success('Client secret rotated. Copy the new value now.');
        },
        error: (err: any) => {
          this.notify.error(err.error?.message || 'Failed to rotate client secret.');
        }
      });
  }

  copyText(value: string | null | undefined, label: string) {
    const normalized = (value || '').trim();
    if (!normalized) {
      this.notify.error(`No ${label.toLowerCase()} available to copy.`);
      return;
    }

    navigator.clipboard.writeText(normalized)
      .then(() => this.notify.success(`${label} copied.`))
      .catch(() => this.notify.error(`Failed to copy ${label.toLowerCase()}.`));
  }
}
