import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RolesService } from '../../core/services/roles.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { ConfirmService } from '../../core/services/confirm.service';
import type { PermissionDto, RoleDto } from '../../core/models';
import { PermissionsService } from '../../core/services/permissions.service';
import {
  SearchableMultiSelectComponent,
  type SearchableMultiSelectOption
} from '../../shared/components/searchable-multi-select/searchable-multi-select.component';
import { ListPageSizeService } from '../../core/services/list-page-size.service';
import { paginateSlice, totalListPages } from '../../core/utils/pagination';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';

@Component({
  selector: 'app-roles',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    DrawerComponent,
    FormFieldComponent,
    DrawerFooterActionsComponent,
    IconButtonComponent,
    LoadingStateComponent,
    SearchableMultiSelectComponent,
    TablePaginationComponent
  ],
  templateUrl: './roles.component.html'
})
export class RolesComponent implements OnInit {
  private rolesService = inject(RolesService);
  private permissionsService = inject(PermissionsService);
  private notify = inject(NotificationService);
  private confirm = inject(ConfirmService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);
  readonly listPageSizeSvc = inject(ListPageSizeService);

  roles = signal<RoleDto[]>([]);
  loading = signal(true);
  error = signal('');
  search = signal('');
  showModal = signal<'create' | 'edit' | null>(null);
  closing = signal(false);
  editingRole = signal<RoleDto | null>(null);
  saving = signal(false);
  deleting = signal<string | null>(null);

  permissionsCatalog = signal<PermissionDto[]>([]);
  permissionsLoading = signal(false);
  selectedPermissionCodes = signal<string[]>([]);

  form = { name: '', description: '' };

  sortedPermissions = computed(() =>
    [...this.permissionsCatalog()].sort((a, b) => a.code.localeCompare(b.code))
  );

  permissionSelectOptions = computed<SearchableMultiSelectOption[]>(() =>
    this.sortedPermissions().map((p) => ({
      value: p.code,
      label: p.code,
      description: [p.description, p.category].filter(Boolean).join(' · ') || undefined
    }))
  );

  filteredRoles = computed(() => {
    const list = this.roles();
    const term = this.search().toLowerCase().trim();
    if (!term) return list;
    return list.filter(
      r =>
        r.name.toLowerCase().includes(term) ||
        (r.description || '').toLowerCase().includes(term) ||
        (r.permissions || '').toLowerCase().includes(term)
    );
  });

  listPage = signal(1);
  readonly pageSize = computed(() => this.listPageSizeSvc.pageSize());
  totalFiltered = computed(() => this.filteredRoles().length);
  paginatedRoles = computed(() => paginateSlice(this.filteredRoles(), this.listPage(), this.pageSize()));

  constructor() {
    effect(() => {
      const total = this.filteredRoles().length;
      const size = this.pageSize();
      const max = totalListPages(total, size);
      if (this.listPage() > max) {
        this.listPage.set(max);
      }
    });
  }

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Authorization',
      description: this.translate.t().rolesPageDesc
    });
    this.loadRoles();
    this.loadPermissionsCatalog();
  }

  onSearchChange(value: string) {
    this.search.set(value);
    this.listPage.set(1);
  }

  setListPage(p: number) {
    this.listPage.set(p);
  }

  onListPageSizeChange(size: number) {
    this.listPageSizeSvc.setPageSize(size);
    this.listPage.set(1);
  }

  loadPermissionsCatalog() {
    this.permissionsLoading.set(true);
    this.permissionsService.getPermissions().subscribe({
      next: (list: PermissionDto[]) => {
        this.permissionsCatalog.set(list);
        this.permissionsLoading.set(false);
      },
      error: (err: any) => {
        this.permissionsCatalog.set([]);
        this.permissionsLoading.set(false);
      }
    });
  }

  /** Chuỗi permissions gửi API (comma-separated codes), undefined nếu không chọn. */
  private permissionsPayload(): string | undefined {
    const codes = this.selectedPermissionCodes();
    if (!codes.length) return undefined;
    const catalog = this.permissionsCatalog();
    const canonical = codes.map((raw) => {
      const match = catalog.find((p) => p.code.toLowerCase() === raw.toLowerCase());
      return match?.code ?? raw;
    });
    canonical.sort((a, b) => a.localeCompare(b));
    return [...new Set(canonical)].join(',');
  }

  private setSelectedFromRolePermissions(permissionsCsv: string | null | undefined) {
    const raw = (permissionsCsv || '')
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);
    const catalog = this.permissionsCatalog();
    const resolved = raw.map((r) => {
      const match = catalog.find((p) => p.code.toLowerCase() === r.toLowerCase());
      return match?.code ?? r;
    });
    this.selectedPermissionCodes.set([...new Set(resolved)]);
  }

  loadRoles() {
    this.loading.set(true);
    this.rolesService.getRoles().subscribe({
      next: (list: RoleDto[]) => {
        this.roles.set(list);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Không thể tải danh sách roles');
        this.loading.set(false);
      }
    });
  }

  openCreate() {
    this.form = { name: '', description: '' };
    this.selectedPermissionCodes.set([]);
    this.editingRole.set(null);
    this.showModal.set('create');
  }

  openEdit(role: RoleDto) {
    this.form = {
      name: role.name,
      description: role.description || ''
    };
    this.setSelectedFromRolePermissions(role.permissions);
    this.editingRole.set(role);
    this.showModal.set('edit');
  }

  closeModal() {
    if (this.closing()) return;
    this.closing.set(true);
    setTimeout(() => {
      this.showModal.set(null);
      this.editingRole.set(null);
      this.closing.set(false);
    }, 300);
  }

  saveCreate() {
    if (!this.form.name.trim()) {
      this.notify.error('Tên role không được để trống');
      return;
    }
    this.saving.set(true);
    const perms = this.permissionsPayload();
    this.rolesService
      .createRole({
        name: this.form.name.trim(),
        description: this.form.description.trim(),
        ...(perms !== undefined ? { permissions: perms } : {})
      })
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.notify.success('Tạo role thành công');
            this.loadRoles();
            this.closeModal();
          } else {
            this.notify.error(res.message || 'Tạo role thất bại');
          }
          this.saving.set(false);
        },
        error: (err: any) => {
          this.notify.error(err.error?.message || 'Tạo role thất bại');
          this.saving.set(false);
        }
      });
  }

  saveEdit() {
    const role = this.editingRole();
    if (!role) return;
    this.saving.set(true);
    this.rolesService
      .updateRole(role.id, {
        description: this.form.description.trim(),
        permissions: this.permissionsPayload() ?? ''
      })
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.notify.success('Cập nhật role thành công');
            this.loadRoles();
            this.closeModal();
          } else {
            this.notify.error(res.message || 'Cập nhật thất bại');
          }
          this.saving.set(false);
        },
        error: (err: any) => {
          this.notify.error(err.error?.message || 'Cập nhật thất bại');
          this.saving.set(false);
        }
      });
  }

  async deleteRole(role: RoleDto) {
    if (role.isSystemRole) {
      this.notify.error('Không thể xóa role hệ thống');
      return;
    }
    const ok = await this.confirm.confirm({
      title: 'Xóa role',
      message: `Bạn chắc chắn muốn xóa role "${role.name}"?`,
      confirmText: 'Xóa',
      cancelText: 'Hủy',
      variant: 'danger'
    });
    if (!ok) return;
    this.deleting.set(role.id);
    this.rolesService.deleteRole(role.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã xóa role');
          this.loadRoles();
        } else {
          this.notify.error(res.message || 'Xóa thất bại');
        }
        this.deleting.set(null);
      },
      error: (err: any) => {
        this.notify.error(err.error?.message || 'Xóa thất bại');
        this.deleting.set(null);
      }
    });
  }
}
