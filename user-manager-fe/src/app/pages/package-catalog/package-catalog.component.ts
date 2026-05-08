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
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { PlanPriceVndPipe } from '../../shared/pipes/plan-price-vnd.pipe';
import {
  MultiSelectDropdownComponent,
  type MultiSelectOption
} from '../../shared/components/multi-select-dropdown/multi-select-dropdown.component';
import type { ApplicationRoleDto, LicensePackageCatalogDto, SubscriptionPlanCatalogDto } from '../../core/models/api-response';
import { assignedRolesToCsv, DEFAULT_ASSIGNED_ROLE, parseAssignedRolesCsv } from '../../core/utils/assigned-roles';
import { parsePlanLimitField } from '../../core/utils/plan-limit-fields';

@Component({
  selector: 'app-package-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownComponent,
    FormFieldComponent,
    LoadingStateComponent,
    IconButtonComponent,
    DrawerComponent,
    DrawerFooterActionsComponent,
    MultiSelectDropdownComponent,
    PlanPriceVndPipe
  ],
  templateUrl: './package-catalog.component.html'
})
export class PackageCatalogComponent implements OnInit {
  private applicationsService = inject(ApplicationsService);
  private subscriptionsService = inject(SubscriptionsService);
  private notify = inject(NotificationService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);

  applicationId = signal('');
  apps = signal<ApplicationListItem[]>([]);
  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  packages = signal<LicensePackageCatalogDto[]>([]);
  loading = signal(false);
  search = signal('');

  showPlanModal = signal(false);
  closingPlan = signal(false);
  savingPlan = signal(false);
  editingPlanId = signal<string | null>(null);
  appRoles = signal<ApplicationRoleDto[]>([]);
  /** Role cho dropdown “Assigned roles” của form gói trong drawer (theo application trong form). */
  planModalApplicationId = signal('');
  planModalAppRoles = signal<ApplicationRoleDto[]>([]);
  loadingPlanModalAppRoles = signal(false);
  loadingApps = signal(false);
  private planRoleFallbackCsv: string | null = null;

  showLicenseModal = signal(false);
  closingLicense = signal(false);
  savingLicense = signal(false);
  editingLicenseId = signal<string | null>(null);
  licensePlanId = signal<string>('');
  newLicense: any = {
    name: '',
    assignedRoles: [] as string[],
    seatLimit: -1,
    isActive: true,
    displayOrder: 0,
    featureAnalytics: false,
    featureSupport: false,
    featureAdvancedSecurity: false,
    featureCustomDomain: false
  };

  appOptions = computed<DropdownOption[]>(() => {
    const list = this.apps();
    return [{ value: '', label: 'Chọn application' }, ...list.map(a => ({ value: a.id, label: a.name }))];
  });

  filteredPlans = computed(() => {
    const q = this.search().trim().toLowerCase();
    let list = this.plans();
    if (!q) return list;
    return list.filter(
      p =>
        p.name.toLowerCase().includes(q) ||
        (p.description || '').toLowerCase().includes(q) ||
        (p.assignedRole || '').toLowerCase().includes(q)
    );
  });

  orphanPackages = computed(() =>
    this.packages().filter(p => !p.subscriptionPlanId || !this.plans().some(pl => pl.id === p.subscriptionPlanId))
  );

  /** Role của application đã chọn trong form gói (+ role cũ không còn trong app). */
  planAssignedRoleOptions(): MultiSelectOption[] {
    const appId = this.planModalApplicationId().trim();
    if (!appId) return [];
    const base = this.planModalAppRoles().map(r => ({
      value: r.id,
      label: r.name,
      variant: 'default' as const
    }));
    const extras = this.planExtraRolesNotInModalApp().map(id => ({
      value: id,
      label: `${id.slice(0, 8)}… (không trong app)`,
      variant: 'warning' as const
    }));
    return [...base, ...extras];
  }

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
    applicationId: '',
    assignedRoleIds: [] as string[],
    icon: null
  };

  ngOnInit(): void {
    this.pageToolbar.setMeta({
      badge: 'Catalog',
      description: this.translate.t().packageCatalogDesc
    });
    this.refreshApplications();
  }

  /** Tải lại danh sách application (dùng khi vào trang / mở drawer tạo-sửa gói). */
  refreshApplications() {
    this.loadingApps.set(true);
    this.applicationsService.getApplications().subscribe({
      next: list => {
        this.apps.set(list);
        this.loadingApps.set(false);
      },
      error: () => {
        this.apps.set([]);
        this.loadingApps.set(false);
      }
    });
  }

  /** Khi mở drawer: tải lại danh sách application. */
  refreshModalData() {
    this.refreshApplications();
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

  licensesForPlan(planId: string): LicensePackageCatalogDto[] {
    return this.packages().filter(p => p.subscriptionPlanId === planId);
  }

  loadCatalog() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    this.loading.set(true);
    let done = 0;
    const finish = () => {
      done++;
      if (done >= 3) this.loading.set(false);
    };

    this.subscriptionsService.getPlans(appId).subscribe({
      next: res => this.plans.set(res.success && res.data ? res.data : []),
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
        const first = roles[0]?.name ?? '';
        const firstId = roles[0]?.id ?? '';
        if (!this.newPlan.assignedRoleIds?.length) {
          this.newPlan.assignedRoleIds = firstId ? [firstId] : [];
        }
        if (!this.newLicense.assignedRoles?.length) {
          this.newLicense.assignedRoles = first ? [first] : [DEFAULT_ASSIGNED_ROLE];
        }
      },
      error: () => {
        this.appRoles.set([]);
        finish();
      },
      complete: finish
    });
  }

  getApplicationName(): string {
    const id = this.applicationId();
    if (!id) return '';
    return this.apps().find(a => a.id === id)?.name ?? id;
  }

  /** Role hiển thị trên thẻ gói / license (CSV → danh sách; rỗng → Guest). */
  planRolesDisplay(csv: string | null | undefined): string[] {
    const parts = parseAssignedRolesCsv(csv);
    return parts.length ? parts : [DEFAULT_ASSIGNED_ROLE];
  }

  /** -1 / null → «Tất cả» cho quota. */
  displayQuota(value: number | null | undefined): string {
    if (value === -1 || value == null) return 'Tất cả';
    return String(value);
  }

  planExtraRolesNotInModalApp(): string[] {
    const ids = new Set(this.planModalAppRoles().map(r => r.id));
    return (this.newPlan.assignedRoleIds ?? []).filter((rid: string) => !ids.has(rid));
  }

  licenseExtraRolesNotInApp(): string[] {
    const g = new Set(this.appRoles().map(r => r.name));
    return (this.newLicense.assignedRoles ?? []).filter((n: string) => !g.has(n));
  }

  onPlanApplicationChange(appId: string) {
    const id = (appId || '').trim();
    this.planModalApplicationId.set(id);
    this.newPlan.applicationId = id;
    if (!id) {
      this.planModalAppRoles.set([]);
      this.loadingPlanModalAppRoles.set(false);
      this.newPlan.assignedRoleIds = [];
      this.planRoleFallbackCsv = null;
      return;
    }
    this.loadPlanModalApplicationRoles(id);
  }

  private loadPlanModalApplicationRoles(applicationId: string) {
    this.planModalAppRoles.set([]);
    this.loadingPlanModalAppRoles.set(true);
    this.applicationsService.getApplicationRoles(applicationId).subscribe({
      next: roles => {
        this.planModalAppRoles.set(roles);
        const idSet = new Set(roles.map(r => r.id));
        let next = (this.newPlan.assignedRoleIds ?? []).filter((rid: string) => idSet.has(rid));
        if (!next.length && this.planRoleFallbackCsv) {
          const names = parseAssignedRolesCsv(this.planRoleFallbackCsv).map(n => n.toLowerCase());
          next = roles.filter(r => names.includes(r.name.toLowerCase())).map(r => r.id);
        }
        if (!next.length && roles[0]) {
          next = [roles[0].id];
        }
        this.newPlan.assignedRoleIds = next;
        this.loadingPlanModalAppRoles.set(false);
      },
      error: () => {
        this.planModalAppRoles.set([]);
        this.newPlan.assignedRoleIds = [];
        this.loadingPlanModalAppRoles.set(false);
      }
    });
  }

  openCreatePlan() {
    this.editingPlanId.set(null);
    this.resetPlanForm();
    const pageApp = this.applicationId().trim();
    if (pageApp) {
      this.newPlan.applicationId = pageApp;
      this.onPlanApplicationChange(pageApp);
    }
    this.refreshModalData();
    this.closingPlan.set(false);
    this.showPlanModal.set(true);
  }

  openEditPlan(plan: SubscriptionPlanCatalogDto) {
    this.editingPlanId.set(plan.id);
    const hasIds = (plan.assignedRoleIds?.length ?? 0) > 0;
    this.planRoleFallbackCsv = hasIds ? null : plan.assignedRole || null;
    this.newPlan = {
      name: plan.name,
      description: plan.description ?? '',
      monthlyPrice: plan.monthlyPrice,
      yearlyPrice: plan.yearlyPrice,
      maxUsers: parsePlanLimitField(plan.maxUsers),
      maxApiCalls: parsePlanLimitField(plan.maxApiCalls),
      includeAnalytics: plan.includeAnalytics,
      includeSupport: plan.includeSupport,
      includeAdvancedSecurity: plan.includeAdvancedSecurity,
      includeCustomDomain: plan.includeCustomDomain,
      isActive: plan.isActive,
      displayOrder: plan.displayOrder,
      applicationId: plan.applicationId ?? '',
      assignedRoleIds: hasIds ? [...plan.assignedRoleIds!] : [],
      icon: plan.icon
    };
    this.refreshModalData();
    this.onPlanApplicationChange(this.newPlan.applicationId);
    this.closingPlan.set(false);
    this.showPlanModal.set(true);
  }

  closePlanModal() {
    if (this.closingPlan()) return;
    this.closingPlan.set(true);
    setTimeout(() => {
      this.showPlanModal.set(false);
      this.closingPlan.set(false);
    }, 300);
  }

  savePlan() {
    if (!this.newPlan.name?.trim()) return this.notify.error('Vui lòng nhập tên gói');
    const appId = this.newPlan.applicationId?.trim() ?? '';
    if (appId && !(this.newPlan.assignedRoleIds?.length ?? 0)) {
      return this.notify.error('Chọn ít nhất một role application cho gói');
    }
    this.savingPlan.set(true);
    const payload = {
      applicationId: appId || null,
      name: this.newPlan.name.trim(),
      description: this.newPlan.description ?? '',
      monthlyPrice: Number(this.newPlan.monthlyPrice),
      yearlyPrice: Number(this.newPlan.yearlyPrice),
      maxUsers: parsePlanLimitField(this.newPlan.maxUsers),
      maxApiCalls: parsePlanLimitField(this.newPlan.maxApiCalls),
      includeAnalytics: !!this.newPlan.includeAnalytics,
      includeSupport: !!this.newPlan.includeSupport,
      includeAdvancedSecurity: !!this.newPlan.includeAdvancedSecurity,
      includeCustomDomain: !!this.newPlan.includeCustomDomain,
      isActive: !!this.newPlan.isActive,
      displayOrder: Number(this.newPlan.displayOrder),
      assignedRoleIds: [...(this.newPlan.assignedRoleIds ?? [])] as string[],
      icon: this.newPlan.icon ?? null
    };
    const editingId = this.editingPlanId();
    const req$ = editingId
      ? this.subscriptionsService.updatePlan(editingId, {
          applicationId: payload.applicationId,
          name: payload.name,
          description: payload.description,
          monthlyPrice: payload.monthlyPrice,
          yearlyPrice: payload.yearlyPrice,
          maxUsers: payload.maxUsers,
          maxApiCalls: payload.maxApiCalls,
          includeAnalytics: payload.includeAnalytics,
          includeSupport: payload.includeSupport,
          includeAdvancedSecurity: payload.includeAdvancedSecurity,
          includeCustomDomain: payload.includeCustomDomain,
          isActive: payload.isActive,
          displayOrder: payload.displayOrder,
          assignedRoleIds: payload.assignedRoleIds,
          icon: payload.icon
        })
      : this.subscriptionsService.createPlan(payload);
    req$.subscribe({
      next: res => {
        if (!res.success) {
          this.notify.error(res.message || (editingId ? 'Sửa gói thất bại' : 'Tạo gói thất bại'));
          this.savingPlan.set(false);
          return;
        }
        this.notify.success(editingId ? 'Đã cập nhật gói' : 'Đã tạo gói');
        this.closePlanModal();
        this.loadCatalog();
        this.savingPlan.set(false);
      },
      error: err => {
        this.notify.error(err.error?.message || (editingId ? 'Sửa gói thất bại' : 'Tạo gói thất bại'));
        this.savingPlan.set(false);
      }
    });
  }

  private resetPlanForm() {
    const pageApp = this.applicationId().trim();
    this.planModalApplicationId.set('');
    this.planModalAppRoles.set([]);
    this.loadingPlanModalAppRoles.set(false);
    this.planRoleFallbackCsv = null;
    const r0 = this.appRoles()[0];
    this.newPlan = {
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
      applicationId: pageApp,
      assignedRoleIds: pageApp && r0 ? [r0.id] : [],
      icon: null
    };
  }

  openAddLicense(planId: string) {
    const appId = this.applicationId().trim();
    if (!appId) return;
    this.editingLicenseId.set(null);
    this.licensePlanId.set(planId);
    this.resetLicenseForm();
    this.closingLicense.set(false);
    this.showLicenseModal.set(true);
  }

  openEditLicense(lic: LicensePackageCatalogDto) {
    this.editingLicenseId.set(lic.id);
    this.licensePlanId.set(lic.subscriptionPlanId || '');
    const roles = parseAssignedRolesCsv(lic.assignedRole);
    this.newLicense = {
      name: lic.name,
      assignedRoles: roles.length ? roles : [DEFAULT_ASSIGNED_ROLE],
      seatLimit: lic.seatLimit,
      isActive: lic.isActive,
      displayOrder: lic.displayOrder,
      featureAnalytics: lic.features?.includes('Analytics') || false,
      featureSupport: lic.features?.includes('Support') || false,
      featureAdvancedSecurity: lic.features?.includes('AdvancedSecurity') || false,
      featureCustomDomain: lic.features?.includes('CustomDomain') || false
    };
    this.closingLicense.set(false);
    this.showLicenseModal.set(true);
  }

  closeLicenseModal() {
    if (this.closingLicense()) return;
    this.closingLicense.set(true);
    setTimeout(() => {
      this.showLicenseModal.set(false);
      this.closingLicense.set(false);
    }, 300);
  }

  private resetLicenseForm() {
    const appId = this.applicationId();
    const roles = this.appRoles();
    this.newLicense = {
      name: '',
      assignedRoles: roles[0]?.name ? [roles[0].name] : [DEFAULT_ASSIGNED_ROLE],
      seatLimit: -1,
      isActive: true,
      displayOrder: 0,
      featureAnalytics: false,
      featureSupport: false,
      featureAdvancedSecurity: false,
      featureCustomDomain: false
    };
  }

  saveLicense() {
    const appId = this.applicationId().trim();
    const planId = this.licensePlanId().trim();
    if (!appId || !planId) return;
    if (!this.newLicense.name?.trim()) return this.notify.error('Vui lòng nhập tên license');
    if (!this.newLicense.assignedRoles?.length) {
      this.newLicense.assignedRoles = [DEFAULT_ASSIGNED_ROLE];
    }
    const features: string[] = [];
    if (this.newLicense.featureAnalytics) features.push('Analytics');
    if (this.newLicense.featureSupport) features.push('Support');
    if (this.newLicense.featureAdvancedSecurity) features.push('AdvancedSecurity');
    if (this.newLicense.featureCustomDomain) features.push('CustomDomain');
    
    this.savingLicense.set(true);
    const editingId = this.editingLicenseId();
    
    const req$ = editingId 
      ? this.subscriptionsService.updatePackage(editingId, {
          applicationId: appId,
          name: this.newLicense.name.trim(),
          description: null,
          features: features,
          seatLimit: Number(this.newLicense.seatLimit),
          isActive: !!this.newLicense.isActive,
          displayOrder: Number(this.newLicense.displayOrder),
          assignedRole: assignedRolesToCsv(this.newLicense.assignedRoles)
        })
      : this.subscriptionsService.createPackage({
          applicationId: appId,
          subscriptionPlanId: planId,
          name: this.newLicense.name.trim(),
          description: null,
          features: features,
          seatLimit: Number(this.newLicense.seatLimit),
          isActive: !!this.newLicense.isActive,
          displayOrder: Number(this.newLicense.displayOrder),
          assignedRole: assignedRolesToCsv(this.newLicense.assignedRoles)
        });

    req$.subscribe({
        next: res => {
          if (!res.success) {
            this.notify.error(res.message || (editingId ? 'Sửa license thất bại' : 'Tạo license thất bại'));
            this.savingLicense.set(false);
            return;
          }
          this.notify.success(editingId ? 'Đã cập nhật license' : 'Đã tạo license trong gói');
          this.closeLicenseModal();
          this.loadCatalog();
          this.savingLicense.set(false);
        },
        error: err => {
          this.notify.error(err.error?.message || (editingId ? 'Sửa license thất bại' : 'Tạo license thất bại'));
          this.savingLicense.set(false);
        }
      });
  }
}
