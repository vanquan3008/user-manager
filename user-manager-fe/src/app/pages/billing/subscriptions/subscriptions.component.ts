import { PlanPriceVndPipe } from '../../../shared/pipes/plan-price-vnd.pipe';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { Component, DestroyRef, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SubscriptionsService } from '../../../core/services/subscriptions.service';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { NotificationService } from '../../../core/services/notification.service';
import { formatDateTime } from '../../../shared/utils/format';
import { apiErrorMessage } from '../../../core/utils/http-error.utils';
import { PageToolbarService } from '../../../core/services/page-toolbar.service';
import { TranslateService } from '../../../core/services/translate.service';
import { LoadingStateComponent } from '../../../shared/components/loading-state/loading-state.component';
import type { DropdownOption } from '../../../shared/components/dropdown/dropdown.component';
import { DrawerComponent } from '../../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import type { ApplicationListItem, SubscriptionDto, ApplicationUserDto, SubscriptionPlanCatalogDto, SubscriptionType } from '../../../core/models';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, DrawerFooterActionsComponent, LoadingStateComponent, EmptyStateComponent, PlanPriceVndPipe],
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
  readonly t = this.translate.t;
  readonly subscriptionTypes: SubscriptionType[] = ['Free', 'Premium', 'Family', 'Student'];

  planName(planId: string | null | undefined): string {
    return this.plans().find(p => p.id === planId)?.name ?? '—';
  }

  when(iso: string | null | undefined) { return formatDateTime(iso, this.translate.locale()); }

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
        error: (err: any) => this.notify.error(apiErrorMessage(err))
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
        error: (err: any) => this.notify.error(apiErrorMessage(err))
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
        error: (err: any) => this.error.set(apiErrorMessage(err))
      });

    // Load history
    this.subsService.getHistory(appId, userId)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: list => this.history.set(list),
        error: (err: any) => this.notify.error(apiErrorMessage(err))
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
        error: (err: any) => this.notify.error(apiErrorMessage(err))
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
      this.notify.warning(this.t().sbPickPlan);
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
          this.notify.success(this.t().sbActivated);
          this.closeActivateModal();
          this.loadSubscriptions();
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

  async doCancel() {
    const confirmed = await this.confirm.confirm({
      title: this.t().sbCancel,
      message: this.t().sbCancelConfirm,
      confirmText: this.t().sbCancel,
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
          this.notify.success(this.t().sbCancelled);
          this.loadSubscriptions();
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
}
