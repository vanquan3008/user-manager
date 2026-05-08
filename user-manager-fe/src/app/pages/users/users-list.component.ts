import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../core/services/users.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { TranslateService } from '../../core/services/translate.service';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { SsoCreateFormComponent } from '../../shared/components/sso-create-form/sso-create-form.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { ThemeService } from '../../core/services/theme.service';
import { hasAdminRole } from '../../core/constants/roles';
import type { IdentityUser } from '../../core/models/api-response';
import { ListPageSizeService } from '../../core/services/list-page-size.service';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';

@Component({
  selector: 'app-users-list',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    StatCardComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    DrawerComponent,
    SsoCreateFormComponent,
    DrawerFooterActionsComponent,
    IconButtonComponent,
    LoadingStateComponent,
    TablePaginationComponent
  ],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent implements OnInit {
  private usersService = inject(UsersService);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  theme = inject(ThemeService).theme;
  private translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);
  readonly listPageSizeSvc = inject(ListPageSizeService);

  users = signal<IdentityUser[]>([]);
  showSsoModal = signal(false);
  closingSso = signal(false);
  ssoSaving = signal(false);
  ssoForm = {
    email: '',
    password: '',
    username: '',
    firstName: '',
    lastName: '',
    dateOfBirth: ''
  };
  canCreateSso = computed(() => hasAdminRole(this.auth.user()?.roles));
  totalCount = signal(0);
  page = signal(1);
  readonly pageSize = computed(() => this.listPageSizeSvc.pageSize());
  search = signal('');
  loading = signal(false);
  error = signal('');

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  pageActive = computed(() => this.users().filter(u => u.isActive && !u.isBanned).length);
  pageBanned = computed(() => this.users().filter(u => u.isBanned).length);
  pageTotal = computed(() => Math.max(this.users().length, 1));
  activePercent = computed(() => (this.pageActive() / this.pageTotal()) * 100);
  bannedPercent = computed(() => (this.pageBanned() / this.pageTotal()) * 100);

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Identity',
      description: this.translate.t().usersPageDesc
    });
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.usersService.getUsers(this.page(), this.pageSize(), this.search() || undefined).subscribe({
      next: (res) => {
        this.users.set(res.data || []);
        this.totalCount.set(res.totalCount ?? 0);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || err.error?.error?.message || 'Failed to load users');
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    this.page.set(1);
    this.load();
  }

  goToPage(p: number) {
    const total = this.totalPages();
    if (p < 1 || p > total) return;
    this.page.set(p);
    this.load();
  }

  setListPage(p: number) {
    this.goToPage(p);
  }

  onListPageSizeChange(size: number) {
    this.listPageSizeSvc.setPageSize(size);
    this.page.set(1);
    this.load();
  }

  trackByUserId(_: number, u: IdentityUser) {
    return u.id;
  }

  openSsoModal() {
    this.ssoForm = { email: '', password: '', username: '', firstName: '', lastName: '', dateOfBirth: '' };
    this.closingSso.set(false);
    this.showSsoModal.set(true);
  }

  closeSsoModal() {
    if (this.closingSso()) return;
    this.closingSso.set(true);
    setTimeout(() => {
      this.showSsoModal.set(false);
      this.closingSso.set(false);
    }, 300);
  }

  createSsoUser() {
    const f = this.ssoForm;
    if (!f.email?.trim() || !f.password?.trim() || !f.username?.trim()) {
      this.notify.error('Email, mật khẩu và username là bắt buộc.');
      return;
    }
    if (!f.dateOfBirth?.trim()) {
      this.notify.error('Vui lòng chọn ngày sinh.');
      return;
    }
    this.ssoSaving.set(true);
    this.usersService.createSsoUser({
      email: f.email.trim(),
      password: f.password,
      username: f.username.trim(),
      firstName: f.firstName.trim(),
      lastName: f.lastName.trim(),
      dateOfBirth: f.dateOfBirth.trim()
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã tạo SSO user thành công.');
          this.closeSsoModal();
          this.load();
        } else {
          this.notify.error(res.message || 'Tạo SSO user thất bại.');
        }
        this.ssoSaving.set(false);
      },
      error: (err) => {
        this.notify.error(err.error?.message || err.error?.error?.message || 'Tạo SSO user thất bại.');
        this.ssoSaving.set(false);
      }
    });
  }
}
