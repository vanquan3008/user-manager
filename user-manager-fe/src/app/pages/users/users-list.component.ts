import { ActivatedRoute, Router } from '@angular/router';
import { ClockService } from '../../core/services/clock.service';
import { TranslateService } from '../../core/services/translate.service';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { initials, relativeTime } from '../../shared/utils/format';
import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../core/services/users.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { SsoCreateFormComponent } from '../../shared/components/sso-create-form/sso-create-form.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { hasAdminRole } from '../../core/constants/roles';
import type { IdentityUser } from '../../core/models';
import { ListPageSizeService } from '../../core/services/list-page-size.service';
import { TablePaginationComponent } from '../../shared/components/table-pagination/table-pagination.component';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    EmptyStateComponent,
    DrawerComponent,
    SsoCreateFormComponent,
    DrawerFooterActionsComponent,
    LoadingStateComponent,
    TablePaginationComponent
  ],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent implements OnInit {
  private readonly clock = inject(ClockService);
  private usersService = inject(UsersService);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  /** Route data `accountType: 'sso'` turns this page into the SSO accounts directory. */
  readonly accountType = (this.route.snapshot.data['accountType'] as 'sso' | 'console' | undefined) ?? undefined;
  readonly title = computed(() => (this.accountType === 'sso' ? this.t().ssoAccountsTitle : this.t().usersTitle));
  readonly subtitle = computed(() => (this.accountType === 'sso' ? this.t().ssoAccountsSubtitle : this.t().usersSubtitle));
  private translate = inject(TranslateService);
  readonly t = this.translate.t;
  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  readonly listPageSizeSvc = inject(ListPageSizeService);

  users = signal<IdentityUser[]>([]);
  showSsoModal = signal(false);
  closingSso = signal(false);
  ssoSaving = signal(false);
  showUserDrawer = signal(false);
  closingUserDrawer = signal(false);
  selectedUser = signal<IdentityUser | null>(null);
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
  pageSso = computed(() => this.users().filter(u => u.isSsoAccount).length);
  pagePending = computed(() => this.users().filter(u => !u.emailConfirmed).length);
  selectedUserLabel = computed(() =>
    this.selectedUser()?.userName || this.selectedUser()?.email || 'User profile'
  );

  ngOnInit() {
    this.load();
  }

  onSearchInput(value: string) {
    this.search.set(value);
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.onSearch(), 300);
  }

  openUser(user: IdentityUser) {
    this.router.navigate(['/users', user.id]);
  }

  isLocked(user: IdentityUser) {
    return !!user.lockoutEnd && new Date(user.lockoutEnd).getTime() > Date.now();
  }

  ago(iso: string) { return relativeTime(iso, this.translate.locale(), this.clock.now()); }
  initialsOf = initials;

  load() {
    this.loading.set(true);
    this.error.set('');
    this.usersService.getUsers(this.page(), this.pageSize(), this.search() || undefined, this.accountType).subscribe({
      next: (res) => {
        this.users.set(res.data || []);
        this.totalCount.set(res.totalCount ?? 0);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err));
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

  openUserDrawer(user: IdentityUser) {
    this.selectedUser.set(user);
    this.closingUserDrawer.set(false);
    this.showUserDrawer.set(true);
  }

  closeUserDrawer() {
    if (this.closingUserDrawer()) return;
    this.closingUserDrawer.set(true);
    setTimeout(() => {
      this.showUserDrawer.set(false);
      this.selectedUser.set(null);
      this.closingUserDrawer.set(false);
    }, 300);
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
      this.notify.error(this.t().usersRequired);
      return;
    }
    if (!f.dateOfBirth?.trim()) {
      this.notify.error(this.t().usersRequired);
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
          this.notify.success(this.t().usersCreated);
          this.closeSsoModal();
          this.load();
        } else {
          this.notify.error(res.message || this.t().authFailed);
        }
        this.ssoSaving.set(false);
      },
      error: (err: any) => {
        this.notify.error(apiErrorMessage(err));
        this.ssoSaving.set(false);
      }
    });
  }

  accountTypeLabel(user: IdentityUser) {
    return user.isSsoAccount ? 'SSO' : 'Standard';
  }

  joinedLabel(user: IdentityUser) {
    return new Intl.DateTimeFormat(this.translate.locale(), { month: 'short', day: 'numeric', year: 'numeric' }).format(
      new Date(user.createdAt)
    );
  }
}
