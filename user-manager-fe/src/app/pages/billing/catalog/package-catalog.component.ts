import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SubscriptionPlanCatalogDto } from '../../../core/models/subscription.model';
import { SubscriptionsService } from '../../../core/services/subscriptions.service';
import { ApplicationsService } from '../../../core/services/applications.service';
import { ApplicationListItem } from '../../../core/models/application.model';
import { FormFieldComponent } from '../../../shared/components/form-field/form-field.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DrawerComponent } from '../../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { PlanPriceVndPipe } from '../../../shared/pipes/plan-price-vnd.pipe';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { PageToolbarService } from '../../../core/services/page-toolbar.service';
import { TranslateService } from '../../../core/services/translate.service';
import { parsePlanLimitField } from '../../../core/utils/plan-limit-fields';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../../core/utils/http-error.utils';

type PlanFormModel = {
  applicationId: string;
  name: string;
  description: string;
  monthlyPrice: number;
  yearlyPrice: number;
  maxUsers: number;
  maxApiCalls: number;
  includeAnalytics: boolean;
  includeSupport: boolean;
  includeAdvancedSecurity: boolean;
  includeCustomDomain: boolean;
  isActive: boolean;
  displayOrder: number;
  icon: string | null;
};

@Component({
  selector: 'app-package-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule, FormFieldComponent, EmptyStateComponent, DrawerComponent, DrawerFooterActionsComponent, PlanPriceVndPipe],
  templateUrl: './package-catalog.component.html'
})
export class PackageCatalogComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);
  private readonly pageToolbar = inject(PageToolbarService);
  private readonly translate = inject(TranslateService);
  readonly t = this.translate.t;

  apps = signal<ApplicationListItem[]>([]);
  loadingApps = signal(false);
  applicationId = signal<string>('');

  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  loading = signal(false);
  search = signal('');

  showPlanModal = signal(false);
  savingPlan = signal(false);
  closingPlan = signal(false);
  editingPlanId = signal<string | null>(null);
  planForm: PlanFormModel = this.createDefaultPlanForm();

  appOptions = computed(() => this.apps().map(app => ({ value: app.id, label: app.name })));
  selectedApp = computed(() => this.apps().find(app => app.id === this.applicationId()) ?? null);
  filteredPlans = computed(() => {
    const term = this.search().toLowerCase().trim();
    const list = this.plans();

    if (!term) {
      return list;
    }

    return list.filter(plan =>
      plan.name.toLowerCase().includes(term) ||
      plan.description?.toLowerCase().includes(term) ||
      this.planFeatures(plan).some(feature => feature.toLowerCase().includes(term))
    );
  });
  sortedPlans = computed(() =>
    [...this.filteredPlans()].sort((left, right) =>
      left.displayOrder - right.displayOrder ||
      left.monthlyPrice - right.monthlyPrice ||
      left.name.localeCompare(right.name)
    )
  );
  totalPlans = computed(() => this.plans().length);
  activePlans = computed(() => this.plans().filter(plan => plan.isActive).length);
  draftPlans = computed(() => this.plans().filter(plan => !plan.isActive).length);
  freePlans = computed(() => this.plans().filter(plan => plan.monthlyPrice === 0 && plan.yearlyPrice === 0).length);

  ngOnInit(): void {
    this.pageToolbar.setMeta({
      badge: 'Catalog',
      description: 'Manage subscription plans per application, including pricing, quotas and bundled features.'
    });
    this.refreshApplications();
  }

  refreshApplications() {
    this.loadingApps.set(true);
    this.applicationsService.getApplications().subscribe({
      next: (apps: ApplicationListItem[]) => {
        this.apps.set(apps);
        if (this.applicationId() && !apps.some(app => app.id === this.applicationId())) {
          this.applicationId.set('');
          this.plans.set([]);
        }
        this.loadingApps.set(false);
      },
      error: () => {
        this.apps.set([]);
        this.loadingApps.set(false);
      }
    });
  }

  onApplicationChange(applicationId: string) {
    this.applicationId.set(applicationId);
    this.search.set('');

    if (!applicationId.trim()) {
      this.plans.set([]);
      return;
    }

    this.loadCatalog();
  }

  loadCatalog() {
    const applicationId = this.applicationId().trim();
    if (!applicationId) {
      return;
    }

    this.loading.set(true);
    this.subscriptionsService.getPlans(applicationId).subscribe({
      next: response => this.plans.set(response.success && response.data ? response.data : []),
      error: () => {
        this.plans.set([]);
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }

  getApplicationName(): string {
    const applicationId = this.applicationId().trim();
    return this.apps().find(app => app.id === applicationId)?.name ?? applicationId;
  }

  planFeatures(plan: SubscriptionPlanCatalogDto): string[] {
    return [
      plan.features?.includes('ANALYTICS') ? 'Analytics' : null,
      plan.features?.includes('SUPPORT') ? 'Support' : null,
      plan.features?.includes('ADVANCED_SECURITY') ? 'Advanced security' : null,
      plan.features?.includes('CUSTOM_DOMAIN') ? 'Custom domain' : null
    ].filter((feature): feature is string => !!feature);
  }

  featureMeta(feature: string): { label: string; icon: string } {
    switch (feature) {
      case 'Analytics':
        return { label: this.t().ctFeatAnalytics, icon: 'query_stats' };
      case 'Support':
        return { label: this.t().ctFeatSupport, icon: 'support_agent' };
      case 'Advanced security':
        return { label: this.t().ctFeatSecurity, icon: 'shield_lock' };
      case 'Custom domain':
        return { label: this.t().ctFeatDomain, icon: 'language' };
      default:
        return { label: feature, icon: 'check_circle' };
    }
  }

  isIconUrl(icon: string | null | undefined): boolean {
    if (!icon) {
      return false;
    }

    return icon.startsWith('http://') || icon.startsWith('https://') || icon.startsWith('/') || icon.startsWith('data:');
  }

  displayQuota(value: number | null | undefined): string {
    if (value == null || value === -1) {
      return '∞';
    }

    return String(value);
  }

  annualSavingsPercent(plan: SubscriptionPlanCatalogDto): number | null {
    if (plan.monthlyPrice <= 0 || plan.yearlyPrice <= 0) {
      return null;
    }

    const yearlyFromMonthly = plan.monthlyPrice * 12;
    if (yearlyFromMonthly <= plan.yearlyPrice) {
      return null;
    }

    return Math.round(((yearlyFromMonthly - plan.yearlyPrice) / yearlyFromMonthly) * 100);
  }

  openCreatePlan() {
    this.editingPlanId.set(null);
    this.planForm = this.createDefaultPlanForm();
    this.closingPlan.set(false);
    this.showPlanModal.set(true);
  }

  openEditPlan(plan: SubscriptionPlanCatalogDto) {
    this.editingPlanId.set(plan.id);
    this.planForm = {
      applicationId: plan.applicationId || '',
      name: plan.name,
      description: plan.description ?? '',
      monthlyPrice: plan.monthlyPrice,
      yearlyPrice: plan.yearlyPrice,
      maxUsers: plan.maxUsers,
      maxApiCalls: plan.maxApiCalls ?? -1,
      includeAnalytics: plan.features?.includes('ANALYTICS') ?? false,
      includeSupport: plan.features?.includes('SUPPORT') ?? false,
      includeAdvancedSecurity: plan.features?.includes('ADVANCED_SECURITY') ?? false,
      includeCustomDomain: plan.features?.includes('CUSTOM_DOMAIN') ?? false,
      isActive: plan.isActive,
      displayOrder: plan.displayOrder,
      icon: plan.icon
    };
    this.closingPlan.set(false);
    this.showPlanModal.set(true);
  }

  savePlan() {
    if (!this.planForm.name.trim()) {
      this.notify.warning(this.t().ctNameRequired);
      return;
    }

    const applicationId = this.planForm.applicationId || this.applicationId();
    if (!applicationId) {
      this.notify.warning(this.t().ctPickApp);
      return;
    }

    const payload = {
      applicationId,
      name: this.planForm.name.trim(),
      description: this.planForm.description.trim(),
      monthlyPrice: Math.max(0, Number(this.planForm.monthlyPrice) || 0),
      yearlyPrice: Math.max(0, Number(this.planForm.yearlyPrice) || 0),
      maxUsers: parsePlanLimitField(this.planForm.maxUsers),
      maxApiCalls: parsePlanLimitField(this.planForm.maxApiCalls),
      features: [
        this.planForm.includeAnalytics ? 'ANALYTICS' : null,
        this.planForm.includeSupport ? 'SUPPORT' : null,
        this.planForm.includeAdvancedSecurity ? 'ADVANCED_SECURITY' : null,
        this.planForm.includeCustomDomain ? 'CUSTOM_DOMAIN' : null
      ].filter((feature): feature is string => !!feature),
      permissions: [],
      isActive: this.planForm.isActive,
      displayOrder: Number(this.planForm.displayOrder) || 0,
      icon: this.planForm.icon?.trim() || null
    };

    this.savingPlan.set(true);
    const planId = this.editingPlanId();
    const request = planId
      ? this.subscriptionsService.updatePlan(planId, payload)
      : this.subscriptionsService.createPlan(payload);

    request.subscribe({
      next: response => {
        this.savingPlan.set(false);
        if (response.success) {
          this.notify.success(this.t().ctSaved);
          this.closePlanModal();
          this.loadCatalog();
        } else {
          this.notify.error(response.message || 'Failed to save plan.');
        }
      },
      error: (error: any) => {
        this.savingPlan.set(false);
        this.notify.error(getHttpErrorMessage(error, 'System error while saving the plan.', UNREACHABLE_SERVER_MESSAGE_VI));
      }
    });
  }

  async deletePlan(plan: SubscriptionPlanCatalogDto) {
    const ok = await this.confirm.confirm({
      title: this.t().delete,
      message: this.t().ctDeleteConfirm.replace('{{name}}', plan.name),
      confirmText: this.t().delete,
      variant: 'danger'
    });

    if (!ok) {
      return;
    }

    this.subscriptionsService.deletePlan(plan.id).subscribe({
      next: res => {
        if (res.success) {
          this.notify.success(this.t().ctDeleted);
          this.loadCatalog();
        } else {
          this.notify.error(res.message || 'Failed to delete plan.');
        }
      },
      error: (error: any) => {
        this.notify.error(getHttpErrorMessage(error, 'System error while deleting the plan.', UNREACHABLE_SERVER_MESSAGE_VI));
      }
    });
  }

  closePlanModal() {
    this.closingPlan.set(true);
    setTimeout(() => {
      this.showPlanModal.set(false);
      this.closingPlan.set(false);
    }, 300);
  }

  private createDefaultPlanForm(): PlanFormModel {
    return {
      applicationId: this.applicationId() || '',
      name: '',
      description: '',
      monthlyPrice: 0,
      yearlyPrice: 0,
      maxUsers: -1,
      maxApiCalls: -1,
      includeAnalytics: false,
      includeSupport: false,
      includeAdvancedSecurity: false,
      includeCustomDomain: false,
      isActive: true,
      displayOrder: 0,
      icon: null
    };
  }
}
