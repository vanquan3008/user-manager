import { Component, DestroyRef, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationsService } from '../../core/services/applications.service';
import type { ApplicationCredentialItem, ApplicationListItem } from '../../core/models';
import { FileUploadService } from '../../core/services/file-upload.service';
import { AuthService } from '../../core/services/auth.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { environment } from '../../../environments/environment';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { TranslateService } from '../../core/services/translate.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { ListPageSizeService } from '../../core/services/list-page-size.service';
import { paginateSlice, totalListPages } from '../../core/utils/pagination';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';

type SortMode = 'name' | 'owner' | 'branding';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    DrawerComponent,
    DrawerFooterActionsComponent,
    EmptyStateComponent,
    TablePaginationComponent
  ],
  templateUrl: './applications.component.html'
})
export class ApplicationsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly fileUploadService = inject(FileUploadService);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;
  readonly clientTypes = computed(() => [
    { value: 'Public', label: this.t().appsPublic, desc: this.t().appsPublicDesc },
    { value: 'Confidential', label: this.t().appsConfidential, desc: this.t().appsConfidentialDesc }
  ]);
  private readonly pageToolbar = inject(PageToolbarService);
  readonly listPageSizeSvc = inject(ListPageSizeService);

  readonly apps = signal<ApplicationListItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly search = signal('');
  readonly sortMode = signal<SortMode>('name');
  readonly viewMode = signal<'grid' | 'list'>('grid');
  readonly showModal = signal(false);
  readonly closing = signal(false);
  readonly saving = signal(false);
  readonly createAttempted = signal(false);
  readonly uploadingLogo = signal(false);
  readonly uploadedLogoPreview = signal<string | null>(null);
  readonly deletingId = signal<string | null>(null);
  readonly createdCredentials = signal<ApplicationCredentialItem | null>(null);
  readonly showCreatedCredentials = signal(false);
  readonly closingCreatedCredentials = signal(false);

  form = {
    name: '',
    description: '',
    clientType: 'Public',
    logoUrl: '',
    backchannelLogoutUri: '',
    allowedRedirectUris: ''
  };

  readonly defaultAppLogoDataUri = environment.defaultAppLogoDataUri;
  readonly currentUserId = computed(() => this.auth.user()?.id ?? '');
  readonly totalApps = computed(() => this.apps().length);
  readonly ownedApps = computed(() => this.apps().filter(app => app.ownerId === this.currentUserId()).length);
  readonly brandedApps = computed(() => this.apps().filter(app => !!app.logoUrl).length);
  readonly describedApps = computed(() => this.apps().filter(app => !!app.description?.trim()).length);
  readonly filteredApps = computed(() => {
    const query = this.search().trim().toLowerCase();
    const sortMode = this.sortMode();

    const filtered = this.apps().filter(app => {
      if (!query) return true;
      return [app.name, app.description, app.ownerId, app.ownerDisplayName, app.id]
        .filter((value): value is string => !!value)
        .some(value => value.toLowerCase().includes(query));
    });

    return [...filtered].sort((left, right) => {
      if (sortMode === 'owner') {
        const leftLabel = (left.ownerDisplayName || left.ownerId || '').toLowerCase();
        const rightLabel = (right.ownerDisplayName || right.ownerId || '').toLowerCase();
        return leftLabel.localeCompare(rightLabel);
      }
      if (sortMode === 'branding') {
        return Number(!!right.logoUrl) - Number(!!left.logoUrl);
      }
      return left.name.localeCompare(right.name);
    });
  });

  listPage = signal(1);
  readonly pageSize = computed(() => this.listPageSizeSvc.pageSize());
  totalFiltered = computed(() => this.filteredApps().length);
  paginatedFilteredApps = computed(() => paginateSlice(this.filteredApps(), this.listPage(), this.pageSize()));

  constructor() {
    effect(() => {
      const total = this.filteredApps().length;
      const size = this.pageSize();
      const max = totalListPages(total, size);
      if (this.listPage() > max) {
        this.listPage.set(max);
      }
    });
  }

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Portfolio',
      description: this.translate.t().appsPageDesc
    });
    this.loadApps();
  }

  onCatalogSearchChange(value: string) {
    this.search.set(value);
    this.listPage.set(1);
  }

  onSortModeChange(value: SortMode) {
    this.sortMode.set(value);
    this.listPage.set(1);
  }

  setListPage(p: number) {
    this.listPage.set(p);
  }

  onListPageSizeChange(size: number) {
    this.listPageSizeSvc.setPageSize(size);
    this.listPage.set(1);
  }

  toggleViewMode() {
    this.viewMode.set(this.viewMode() === 'grid' ? 'list' : 'grid');
    this.listPage.set(1);
  }

  loadApps() {
    this.loading.set(true);
    this.error.set('');

    this.applicationsService.getApplications()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: list => {
          this.apps.set(list);
          this.loading.set(false);
        },
        error: (err: any) => {
          this.error.set(apiErrorMessage(err));
          this.loading.set(false);
        }
      });
  }

  async deleteApp(app: ApplicationListItem) {
    if (!app.id) return;

    const ok = await this.confirm.confirm({
      title: this.t().appsDelete,
      message: this.t().appsDeleteConfirm.replace('{{name}}', app.name),
      confirmText: this.t().appsDelete,
      variant: 'danger'
    });

    if (!ok) return;

    this.deletingId.set(app.id);
    this.applicationsService.deleteApplication(app.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          if (res.success) {
            this.apps.update(list => list.filter(item => item.id !== app.id));
            this.notify.success(this.t().appsDeleted);
          } else {
            this.notify.error(res.message || 'Delete failed.');
          }
          this.deletingId.set(null);
        },
        error: (err: any) => {
          this.notify.error(apiErrorMessage(err));
          this.deletingId.set(null);
        }
      });
  }

  openCreate() {
    this.form = { name: '', description: '', clientType: 'Public', logoUrl: '', backchannelLogoutUri: '', allowedRedirectUris: '' };
    this.createAttempted.set(false);
    this.uploadingLogo.set(false);
    this.uploadedLogoPreview.set(null);
    this.showModal.set(true);
    this.closing.set(false);
  }

  closeModal() {
    this.closing.set(true);
    setTimeout(() => {
      this.showModal.set(false);
      this.closing.set(false);
    }, 250);
  }

  saveCreate() {
    this.createAttempted.set(true);
    const name = this.form.name.trim();
    if (!name) {
      this.notify.validation(this.t().appsNameRequired);
      return;
    }

    const ownerId = this.auth.user()?.id;
    if (!ownerId) {
      this.notify.error('Unable to detect current user. Please sign in again.');
      return;
    }

    this.saving.set(true);
    this.applicationsService
      .createApplication({
        name,
        description: this.form.description.trim() || undefined,
        ownerId,
        clientType: this.form.clientType,
        logoUrl: this.form.logoUrl.trim() || undefined,
        backchannelLogoutUri: this.form.backchannelLogoutUri.trim() || undefined,
        allowedRedirectUris: this.form.allowedRedirectUris.trim() || undefined
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          if (res.success && res.data) {
            this.apps.update(list => [res.data!, ...list]);
            this.closeModal();
            if (res.data.clientType === 'Confidential') {
              this.createdCredentials.set({
                applicationId: res.data.id,
                applicationName: res.data.name,
                clientId: res.data.clientId || '',
                clientType: res.data.clientType || 'Confidential',
                clientSecret: res.data.clientSecret ?? null,
                lastClientSecretRotatedAt: res.data.lastClientSecretRotatedAt ?? null,
                allowedRedirectUris: res.data.allowedRedirectUris,
                backchannelLogoutUri: res.data.backchannelLogoutUri
              });
              this.showCreatedCredentials.set(true);
              this.closingCreatedCredentials.set(false);
            }
            this.notify.success(this.t().appsCreated);
          } else {
            this.notify.error(res.message || 'Create application failed.');
          }
          this.saving.set(false);
        },
        error: (err: any) => {
          this.notify.error(apiErrorMessage(err));
          this.saving.set(false);
        }
      });
  }

  onLogoFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notify.error('Please choose a valid image file.');
      input.value = '';
      return;
    }

    const maxBytes = environment.maxImageUploadBytes;
    if (file.size > maxBytes) {
      this.notify.error('Image is too large. Please use a file under 2 MB.');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.uploadedLogoPreview.set(String(reader.result || '') || null);
    };
    reader.onerror = () => this.notify.error('Unable to read the selected image.');
    reader.readAsDataURL(file);

    this.uploadingLogo.set(true);
    this.fileUploadService
      .uploadImage(file, environment.uploadFolderAppLogos)
      .pipe(
        finalize(() => this.uploadingLogo.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: res => {
          if (res.success && res.data?.url) {
            this.form.logoUrl = res.data.url;
            this.uploadedLogoPreview.set(res.data.url);
            this.notify.success('Logo uploaded.');
          } else {
            this.notify.error(res.message || 'Upload failed.');
          }
        },
        error: (err: any) => {
          this.notify.error(err.error?.message || 'Upload failed.');
        }
      });
  }

  updateName(value: string) {
    this.form.name = value;
    if (value.trim()) this.createAttempted.set(false);
  }

  updateDescription(value: string) {
    this.form.description = value;
  }

  updateClientType(value: string) {
    this.form.clientType = value === 'Confidential' ? 'Confidential' : 'Public';
  }

  updateLogoUrl(value: string) {
    this.form.logoUrl = value;
  }

  updateBackchannelLogoutUri(value: string) {
    this.form.backchannelLogoutUri = value;
  }

  updateAllowedRedirectUris(value: string) {
    this.form.allowedRedirectUris = value;
  }

  isNameInvalid(): boolean {
    return this.createAttempted() && !this.form.name.trim();
  }

  closeCreatedCredentialsDrawer() {
    if (this.closingCreatedCredentials()) return;
    this.closingCreatedCredentials.set(true);
    setTimeout(() => {
      this.showCreatedCredentials.set(false);
      this.closingCreatedCredentials.set(false);
      this.createdCredentials.set(null);
    }, 250);
  }

  copyText(value: string | null | undefined, label: string) {
    const normalized = (value || '').trim();
    if (!normalized) {
      this.notify.error(`No ${label.toLowerCase()} available to copy.`);
      return;
    }
    navigator.clipboard.writeText(normalized)
      .then(() => this.notify.success(this.t().copied))
      .catch(() => this.notify.error(`Failed to copy ${label.toLowerCase()}.`));
  }
}
