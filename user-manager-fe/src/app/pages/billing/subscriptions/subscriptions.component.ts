import { Component, DestroyRef, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SubscriptionsService } from '../../../core/services/subscriptions.service';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { PageToolbarService } from '../../../core/services/page-toolbar.service';
import { TranslateService } from '../../../core/services/translate.service';
import { LoadingStateComponent } from '../../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { IconButtonComponent } from '../../../shared/components/icon-button/icon-button.component';
import { DropdownComponent, type DropdownOption } from '../../../shared/components/dropdown/dropdown.component';
import { DrawerComponent } from '../../../shared/components/drawer/drawer.component';
import { FormFieldComponent } from '../../../shared/components/form-field/form-field.component';
import { DrawerFooterActionsComponent } from '../../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import type { ApplicationListItem, SubscriptionDto, ApplicationUserDto, SubscriptionPlanCatalogDto, SubscriptionType } from '../../../core/models';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    LoadingStateComponent,
    EmptyStateComponent,
    StatCardComponent,
    IconButtonComponent,
    DropdownComponent,
    DrawerComponent,
    FormFieldComponent,
    DrawerFooterActionsComponent
  ],
  templateUrl: './subscriptions.component.html'
})
export class SubscriptionsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly subsService = inject(SubscriptionsService);
  private readonly appsService = inject(ApplicationsService);
  private readonly confirm = inject(ConfirmService);
  private readonly notify = inject(NotificationService);
  private readonly pageToolbar = inject(PageToolbarService);
  readonly translate = inject(TranslateService);
  readonly theme = inject(ThemeService).theme;

  // Signals for Step 1 & 2
  readonly applications = signal<ApplicationListItem[]>([]);
  readonly applicationId = signal('');
  readonly users = signal<ApplicationUserDto[]>([]);
  readonly userId = signal('');
  readonly loadingUsers = signal(false);

  // Signals for Detail View
  readonly active = signal<SubscriptionDto | null>(null);
  readonly history = signal<SubscriptionDto[]>([]);
  readonly plans = signal<SubscriptionPlanCatalogDto[]>([]);
  readonly loading = signal(false);
  readonly loadingPlans = signal(false);
  readonly error = signal('');
  readonly saving = signal(false);

  // Modals
  readonly showActivateModal = signal(false);
  readonly closingActivate = signal(false);

  // Forms
  activateForm = {
    subscriptionPlanId: '',
    subscriptionType: 'Free' as SubscriptionType,
    expiresAt: ''
  };

  // Computed
  readonly appOptions = computed<DropdownOption[]>(() => [
    ...this.applications().map(app => ({ value: app.id, label: app.name }))
  ]);

  readonly userOptions = computed<DropdownOption[]>(() => [
    ...this.users().map(u => ({ value: u.userId, label: u.userName || u.email || '' }))
  ]);

  readonly step1Complete = computed(() => !!this.applicationId().trim());
  readonly step2Complete = computed(() => !!this.userId().trim());

  readonly selectedAppName = computed(() => {
    const id = this.applicationId();
    return this.applications().find(a => a.id === id)?.name || '';
  });

  readonly selectedUserSummary = computed(() => {
    const id = this.userId();
    const user = this.users().find(u => u.userId === id);
    if (!user) return '';
    return `${user.userName || 'No Name'} (${user.email})`;
  });

  constructor() {
    // When application changes, clear user and reload user list
    effect(() => {
      const appId = this.applicationId();
      if (appId) {
        this.userId.set('');
        this.loadUsers(appId);
        this.loadPlans(appId);
      }
    }, { allowSignalWrites: true });

    // When user changes, load subscriptions
    effect(() => {
      const appId = this.applicationId();
      const userId = this.userId();
      if (appId && userId) {
        this.loadSubscriptions();
      } else {
        this.active.set(null);
        this.history.set([]);
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Commerce',
      description: this.translate.t().subsPageDesc
    });
    this.loadApplications();
  }

  loadApplications() {
    this.appsService.getApplications()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: list => this.applications.set(list),
        error: (err: any) => this.notify.error('Không thể tải danh sách ứng dụng')
      });
  }

  loadUsers(appId: string) {
    this.loadingUsers.set(true);
    this.appsService.getApplicationUsers(appId)
      .pipe(
        finalize(() => this.loadingUsers.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: list => this.users.set(list),
        error: (err: any) => this.notify.error('Không thể tải danh sách thành viên ứng dụng')
      });
  }

  loadSubscriptions() {
    const appId = this.applicationId();
    const userId = this.userId();
    if (!appId || !userId) return;

    this.loading.set(true);
    this.error.set('');

    // Load active
    this.subsService.getActive(appId, userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => this.active.set(res.data || null),
        error: (err: any) => this.error.set('Không thể tải active subscription')
      });

    // Load history
    this.subsService.getHistory(appId, userId)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: list => this.history.set(list),
        error: (err: any) => this.notify.error('Không thể tải lịch sử subscription')
      });
  }

  loadPlans(appId: string) {
    this.loadingPlans.set(true);
    this.subsService.getPlans(appId)
      .pipe(
        finalize(() => this.loadingPlans.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: res => this.plans.set(res.data || []),
        error: (err: any) => this.notify.error('Không thể tải danh sách plan')
      });
  }

  onApplicationIdChange(id: string) {
    this.applicationId.set(id);
  }

  // Quota display helper
  displayQuota(val: number | undefined | null): string {
    if (val === undefined || val === null || val === -1) return '∞';
    return val.toString();
  }

  planFeatures(plan: SubscriptionPlanCatalogDto): string[] {
    return plan.features || [];
  }

  openActivateModal() {
    this.activateForm = {
      subscriptionPlanId: '',
      subscriptionType: 'Premium',
      expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    };
    this.showActivateModal.set(true);
  }

  onPlanSelect(planId: string) {
    this.activateForm.subscriptionPlanId = planId;
  }

  closeActivateModal() {
    this.closingActivate.set(true);
    setTimeout(() => {
      this.showActivateModal.set(false);
      this.closingActivate.set(false);
    }, 300);
  }

  doActivate() {
    if (!this.activateForm.subscriptionPlanId) {
      this.notify.warning('Vui lòng chọn một plan');
      return;
    }

    this.saving.set(true);
    this.subsService.activate({
      ...this.activateForm,
      maxMembers: 1, // Default to 1 as it's not managed in UI
      applicationId: this.applicationId(),
      userId: this.userId()
    }).subscribe({
      next: res => {
        if (res.success) {
          this.notify.success('Kích hoạt subscription thành công');
          this.closeActivateModal();
          this.loadSubscriptions();
        } else {
          this.notify.error(res.message || 'Kích hoạt thất bại');
        }
        this.saving.set(false);
      },
      error: (err: any) => {
        this.notify.error(err.error?.message || 'Kích hoạt thất bại');
        this.saving.set(false);
      }
    });
  }

  async doCancel() {
    const confirmed = await this.confirm.confirm({
      title: 'Hủy Subscription',
      message: 'Bạn có chắc chắn muốn hủy gói dịch vụ hiện tại? User sẽ mất quyền truy cập nâng cao ngay lập tức.',
      confirmText: 'Xác nhận hủy',
      variant: 'danger'
    });

    if (!confirmed) return;

    this.saving.set(true);
    this.subsService.cancel({
      applicationId: this.applicationId(),
      userId: this.userId()
    }).subscribe({
      next: res => {
        if (res.success) {
          this.notify.success('Đã hủy subscription');
          this.loadSubscriptions();
        } else {
          this.notify.error(res.message || 'Hủy thất bại');
        }
        this.saving.set(false);
      },
      error: (err: any) => {
        this.notify.error(err.error?.message || 'Hủy thất bại');
        this.saving.set(false);
      }
    });
  }
}
