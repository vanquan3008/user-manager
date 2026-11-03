import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionsService } from '../../core/services/permissions.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { NotificationService } from '../../core/services/notification.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { TranslateService } from '../../core/services/translate.service';
import type { DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import type { PermissionDto } from '../../core/models';
import { ListPageSizeService } from '../../core/services/list-page-size.service';
import { paginateSlice, totalListPages } from '../../core/utils/pagination';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';

@Component({
  selector: 'app-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, DrawerFooterActionsComponent, LoadingStateComponent, TablePaginationComponent, EmptyStateComponent],
  templateUrl: './permissions.component.html'
})
export class PermissionsComponent implements OnInit {
  private permissionsService = inject(PermissionsService);
  private notify = inject(NotificationService);
  translate = inject(TranslateService);
  readonly t = this.translate.t;
  private pageToolbar = inject(PageToolbarService);
  readonly listPageSizeSvc = inject(ListPageSizeService);

  permissions = signal<PermissionDto[]>([]);
  loading = signal(true);
  error = signal('');
  search = signal('');
  categoryFilter = signal<string>('');
  showModal = signal(false);
  closing = signal(false);
  saving = signal(false);

  form = { code: '', category: '', scope: '', description: '' };

  categories = computed(() => {
    const list = this.permissions();
    const set = new Set<string>();
    for (const p of list) {
      const cat = p.category || (p.code?.split('.')[0] || '').replace(/^./, (c: string) => c.toUpperCase());
      if (cat) set.add(cat);
    }
    return Array.from(set).sort();
  });

  /** Lựa chọn filter danh mục (app-dropdown); đồng bộ với template `[options]="categoryOptions()"` */
  readonly categoryOptions = computed<DropdownOption[]>(() => {
    const labels = this.translate.t().permissionCategoryLabels;
    const all = this.translate.t().permissionsAllCategories;
    return [
      { value: '', label: all },
      ...this.categories().map((c) => ({
        value: c,
        label: labels[c.toLowerCase()] ?? c
      }))
    ];
  });

  filteredPermissions = computed(() => {
    let list = this.permissions();
    const term = this.search().toLowerCase().trim();
    const cat = this.categoryFilter();
    if (term) {
      list = list.filter(
        p =>
          (p.code || '').toLowerCase().includes(term) ||
          (p.description || '').toLowerCase().includes(term) ||
          (p.category || '').toLowerCase().includes(term)
      );
    }
    if (cat) {
      list = list.filter(
        p =>
          (p.category || (p.code?.split('.')[0] || '')).toLowerCase() === cat.toLowerCase()
      );
    }
    return list;
  });

  listPage = signal(1);
  readonly pageSize = computed(() => this.listPageSizeSvc.pageSize());
  totalFiltered = computed(() => this.filteredPermissions().length);
  paginatedPermissions = computed(() => paginateSlice(this.filteredPermissions(), this.listPage(), this.pageSize()));

  constructor() {
    effect(() => {
      const total = this.filteredPermissions().length;
      const size = this.pageSize();
      const max = totalListPages(total, size);
      if (this.listPage() > max) {
        this.listPage.set(max);
      }
    });
  }

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Access control',
      description: this.translate.t().permissionsDesc
    });
    this.load();
  }

  onSearchChange(value: string) {
    this.search.set(value);
    this.listPage.set(1);
  }

  onCategoryChange(value: string) {
    this.categoryFilter.set(value);
    this.listPage.set(1);
  }

  setListPage(p: number) {
    this.listPage.set(p);
  }

  onListPageSizeChange(size: number) {
    this.listPageSizeSvc.setPageSize(size);
    this.listPage.set(1);
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.permissionsService.getPermissions().subscribe({
      next: (list) => {
        this.permissions.set(list);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err));
        this.loading.set(false);
      }
    });
  }

  openCreate() {
    this.form = { code: '', category: '', scope: '', description: '' };
    this.closing.set(false);
    this.showModal.set(true);
  }

  closeModal() {
    if (this.closing()) return;
    this.closing.set(true);
    setTimeout(() => {
      this.showModal.set(false);
      this.closing.set(false);
    }, 300);
  }

  saveCreate() {
    if (!this.form.code.trim()) {
      this.notify.error(this.t().pmCodeHint);
      return;
    }
    this.saving.set(true);
    this.permissionsService
      .createPermission({
        code: this.form.code.trim(),
        category: this.form.category.trim(),
        scope: this.form.scope.trim() || null,
        description: this.form.description.trim()
      })
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.notify.success(this.t().pmCreated);
            this.load();
            this.closeModal();
          } else {
            this.notify.error(res.message || this.t().authFailed);
          }
          this.saving.set(false);
        },
        error: (err: any) => {
          this.notify.error(apiErrorMessage(err));
          this.saving.set(false);
        }
      });
  }

  categoryDisplayLabel(raw: string | null | undefined): string {
    if (!raw?.trim()) return '—';
    const labels = this.translate.t().permissionCategoryLabels;
    return labels[raw.toLowerCase()] ?? raw;
  }
}
