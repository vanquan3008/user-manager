import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationsService, type ApplicationListItem } from '../../core/services/applications.service';
import { SubscriptionsService } from '../../core/services/subscriptions.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { DropdownComponent, type DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import {
  MultiSelectDropdownComponent,
  type MultiSelectOption
} from '../../shared/components/multi-select-dropdown/multi-select-dropdown.component';
import { PlanPriceVndPipe } from '../../shared/pipes/plan-price-vnd.pipe';
import type { ApplicationRoleDto, LicensePackageCatalogDto, SubscriptionPlanCatalogDto } from '../../core/models/api-response';
import { assignedRolesToCsv, DEFAULT_ASSIGNED_ROLE } from '../../core/utils/assigned-roles';
import { parsePlanLimitField } from '../../core/utils/plan-limit-fields';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../core/utils/http-error.utils';

@Component({
  selector: 'app-subscription-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownComponent,
    FormFieldComponent,
    MultiSelectDropdownComponent,
    PlanPriceVndPipe
  ],
  templateUrl: './subscription-catalog.component.html'
})
export class SubscriptionCatalogComponent implements OnInit {
  private applicationsService = inject(ApplicationsService);
  private subscriptionsService = inject(SubscriptionsService);
  private notify = inject(NotificationService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);

  applicationId = signal('');
  apps = signal<ApplicationListItem[]>([]);
  appRoles = signal<ApplicationRoleDto[]>([]);
  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  packages = signal<LicensePackageCatalogDto[]>([]);
  loading = signal(false);

  appOptions = computed<DropdownOption[]>(() => {
    const list = this.apps();
    return [{ value: '', label: this.translate.t().selectApplication }, ...list.map(a => ({ value: a.id, label: a.name }))];
  });

  planAssignedRoleOptions = computed<MultiSelectOption[]>(() => {
    const base = this.appRoles().map(r => ({
      value: r.id,
      label: r.name,
      variant: 'default' as const
    }));
    const ids = new Set(this.appRoles().map(r => r.id));
    const extras = (this.newPlan.assignedRoleIds ?? [])
      .filter((id: string) => !ids.has(id))
      .map((id: string) => ({
        value: id,
        label: `${id.slice(0, 8)}… (không trong app)`,
        variant: 'warning' as const
      }));
    return [...base, ...extras];
  });

  licenseAssignedRoleOptions = computed<MultiSelectOption[]>(() => {
    const app = this.appRoles().map(r => ({
      value: r.name,
      label: r.name,
      variant: 'default' as const
    }));
    const extras = this.licenseExtraRolesNotInApp().map(name => ({
      value: name,
      label: `${name} (không trong app)`,
      variant: 'warning' as const
    }));
    return [...app, ...extras];
  });

  newPlan: any = {
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
    assignedRoleIds: [] as string[],
    icon: null
  };

  newPackage: any = {
    subscriptionPlanId: '',
    name: '',
    assignedRoles: [DEFAULT_ASSIGNED_ROLE] as string[],
    seatLimit: -1,
    isActive: true,
    displayOrder: 0,
    featureAnalytics: false,
    featureSupport: false,
    featureAdvancedSecurity: false,
    featureCustomDomain: false
  };

  ngOnInit(): void {
    this.pageToolbar.setMeta({
      badge: 'Plan catalog',
      description: this.translate.t().subscriptionCatalogDesc
    });
    this.applicationsService.getApplications().subscribe({
      next: list => this.apps.set(list),
      error: () => this.apps.set([])
    });
  }

  onApplicationChange(appId: string) {
    this.applicationId.set(appId);
    if (!appId) {
      this.plans.set([]);
      this.packages.set([]);
      this.appRoles.set([]);
      return;
    }
    this.loadCatalog();
  }

  licenseExtraRolesNotInApp(): string[] {
    const g = new Set(this.appRoles().map(r => r.name));
    return (this.newPackage.assignedRoles ?? []).filter((n: string) => !g.has(n));
  }

  loadCatalog() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    this.loading.set(true);

    const finish = (() => {
      let count = 0;
      return () => {
        count++;
        if (count >= 3) this.loading.set(false);
      };
    })();

    this.subscriptionsService.getPlans(appId).subscribe({
      next: res => {
        const list = res.success && res.data ? res.data : [];
        this.plans.set(list);
        if (list.length && !list.some(p => p.id === this.newPackage.subscriptionPlanId)) {
          this.newPackage.subscriptionPlanId = list[0].id;
        }
      },
      error: () => {
        this.plans.set([]);
        finish();
      },
      complete: finish
    });

    this.subscriptionsService.getPackages(appId).subscribe({
      next: res => this.packages.set(res.success && res.data ? res.data : []),
      error: () => {
        this.packages.set([]);
        finish();
      },
      complete: finish
    });

    this.applicationsService.getApplicationRoles(appId).subscribe({
      next: roles => {
        this.appRoles.set(roles);
        const firstId = roles[0]?.id ?? '';
        const firstRole = roles[0]?.name ?? '';
        if (!this.newPlan.assignedRoleIds?.length) {
          this.newPlan.assignedRoleIds = firstId ? [firstId] : [];
        }
        if (!this.newPackage.assignedRoles?.length) {
          this.newPackage.assignedRoles = firstRole ? [firstRole] : [DEFAULT_ASSIGNED_ROLE];
        }
      },
      error: () => {
        this.appRoles.set([]);
        finish();
      },
      complete: finish
    });
  }

  createPlan() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    if (!this.newPlan.name?.trim()) {
      this.notify.error('Vui lòng nhập tên subplan');
      return;
    }
    if (!this.newPlan.assignedRoleIds?.length) {
      return this.notify.error('Chọn ít nhất một role application');
    }
    if (!this.validatePlanConstraints()) {
      return;
    }

    this.subscriptionsService.createPlan({
      applicationId: appId,
      name: this.newPlan.name.trim(),
      description: this.newPlan.description ?? '',
      monthlyPrice: Math.max(0, Number(this.newPlan.monthlyPrice) || 0),
      yearlyPrice: Math.max(0, Number(this.newPlan.yearlyPrice) || 0),
      maxUsers: parsePlanLimitField(this.newPlan.maxUsers),
      maxApiCalls: parsePlanLimitField(this.newPlan.maxApiCalls),
      includeAnalytics: !!this.newPlan.includeAnalytics,
      includeSupport: !!this.newPlan.includeSupport,
      includeAdvancedSecurity: !!this.newPlan.includeAdvancedSecurity,
      includeCustomDomain: !!this.newPlan.includeCustomDomain,
      isActive: !!this.newPlan.isActive,
      displayOrder: Number(this.newPlan.displayOrder),
      assignedRoleIds: [...(this.newPlan.assignedRoleIds ?? [])],
      icon: this.newPlan.icon ?? null
    }).subscribe({
      next: res => {
        if (res.success) {
          this.notify.success('Đã tạo subplan');
          this.newPlan.name = '';
          this.newPlan.description = '';
          this.newPlan.monthlyPrice = 0;
          this.newPlan.yearlyPrice = 0;
          this.newPlan.maxUsers = -1;
          this.newPlan.maxApiCalls = -1;
          this.newPlan.displayOrder = 0;
          const r0 = this.appRoles()[0];
          this.newPlan.assignedRoleIds = r0 ? [r0.id] : [];
          this.loadCatalog();
        } else {
          this.notify.error(res.message || 'Tạo subplan thất bại');
        }
      },
      error: err =>
        this.notify.error(
          getHttpErrorMessage(err, 'Tạo subplan thất bại', UNREACHABLE_SERVER_MESSAGE_VI)
        )
    });
  }

  planNameForLicense(p: LicensePackageCatalogDto): string {
    if (!p.subscriptionPlanId) return '—';
    return this.plans().find(pl => pl.id === p.subscriptionPlanId)?.name ?? '—';
  }

  createPackage() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    const planId = (this.newPackage.subscriptionPlanId || '').trim();
    if (!planId) {
      this.notify.error('Tạo hoặc chọn gói (subplan) trước khi tạo packet');
      return;
    }
    if (!this.newPackage.name?.trim()) {
      this.notify.error('Vui lòng nhập tên packet');
      return;
    }
    if (!this.newPackage.assignedRoles?.length) {
      this.newPackage.assignedRoles = [DEFAULT_ASSIGNED_ROLE];
    }
    if (!this.validatePackageConstraints()) {
      return;
    }

    const features: string[] = [];
    if (this.newPackage.featureAnalytics) features.push('Analytics');
    if (this.newPackage.featureSupport) features.push('Support');
    if (this.newPackage.featureAdvancedSecurity) features.push('AdvancedSecurity');
    if (this.newPackage.featureCustomDomain) features.push('CustomDomain');

    this.subscriptionsService.createPackage({
      applicationId: appId,
      subscriptionPlanId: planId,
      name: this.newPackage.name.trim(),
      description: null,
      features: features,
      seatLimit: parsePlanLimitField(this.newPackage.seatLimit),
      isActive: !!this.newPackage.isActive,
      displayOrder: Number(this.newPackage.displayOrder),
      assignedRole: assignedRolesToCsv(this.newPackage.assignedRoles)
    }).subscribe({
      next: res => {
        if (res.success) {
          this.notify.success('Đã tạo packet');
          this.newPackage.name = '';
          this.newPackage.subscriptionPlanId = this.plans()[0]?.id ?? '';
          this.newPackage.seatLimit = -1;
          this.newPackage.featureAnalytics = false;
          this.newPackage.featureSupport = false;
          this.newPackage.featureAdvancedSecurity = false;
          this.newPackage.featureCustomDomain = false;
          this.loadCatalog();
        } else {
          this.notify.error(res.message || 'Tạo packet thất bại');
        }
      },
      error: err =>
        this.notify.error(
          getHttpErrorMessage(err, 'Tạo packet thất bại', UNREACHABLE_SERVER_MESSAGE_VI)
        )
    });
  }

  private validatePlanConstraints(): boolean {
    const monthlyPrice = Number(this.newPlan.monthlyPrice);
    const yearlyPrice = Number(this.newPlan.yearlyPrice);
    const maxUsers = parsePlanLimitField(this.newPlan.maxUsers);
    const maxApiCalls = parsePlanLimitField(this.newPlan.maxApiCalls);

    if (Number.isNaN(monthlyPrice) || monthlyPrice < 0) {
      this.notify.error('Giá tháng phải lớn hơn hoặc bằng 0');
      return false;
    }
    if (Number.isNaN(yearlyPrice) || yearlyPrice < 0) {
      this.notify.error('Giá năm phải lớn hơn hoặc bằng 0');
      return false;
    }
    if (maxUsers < -1) {
      this.notify.error('MaxUsers chỉ nhận -1 hoặc số không âm');
      return false;
    }
    if (maxApiCalls < -1) {
      this.notify.error('MaxApiCalls chỉ nhận -1 hoặc số không âm');
      return false;
    }
    return true;
  }

  private validatePackageConstraints(): boolean {
    const seatLimit = parsePlanLimitField(this.newPackage.seatLimit);
    if (seatLimit < -1) {
      this.notify.error('SeatLimit chỉ nhận -1 hoặc số không âm');
      return false;
    }
    const assignedRoleCsv = assignedRolesToCsv(this.newPackage.assignedRoles);
    if (!assignedRoleCsv || assignedRoleCsv.length > 100) {
      this.notify.error('Assigned role quá dài (tối đa 100 ký tự)');
      return false;
    }
    return true;
  }
}

