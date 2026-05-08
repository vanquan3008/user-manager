import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationsService, type ApplicationListItem } from '../../core/services/applications.service';
import { SubscriptionsService } from '../../core/services/subscriptions.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { DropdownComponent, type DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { PlanPriceVndPipe } from '../../shared/pipes/plan-price-vnd.pipe';
import {
  MultiSelectDropdownComponent,
  type MultiSelectOption
} from '../../shared/components/multi-select-dropdown/multi-select-dropdown.component';
import type { ApplicationRoleDto, SubscriptionPlanCatalogDto } from '../../core/models/api-response';
import { parseAssignedRolesCsv } from '../../core/utils/assigned-roles';
import { parsePlanLimitField } from '../../core/utils/plan-limit-fields';

@Component({
  selector: 'app-plan-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownComponent,
    FormFieldComponent,
    PageHeaderComponent,
    LoadingStateComponent,
    IconButtonComponent,
    DrawerComponent,
    DrawerFooterActionsComponent,
    MultiSelectDropdownComponent,
    PlanPriceVndPipe
  ],
  templateUrl: './plan-catalog.component.html'
})
export class PlanCatalogComponent implements OnInit {
  private applicationsService = inject(ApplicationsService);
  private subscriptionsService = inject(SubscriptionsService);
  private notify = inject(NotificationService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);

  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  loading = signal(false);
  search = signal('');
  showCreateModal = signal(false);
  closingCreate = signal(false);
  savingCreate = signal(false);
  editingPlanId = signal<string | null>(null);
  apps = signal<ApplicationListItem[]>([]);
  /** Application đang chọn trong drawer (đồng bộ dropdown) — dùng để tính options role. */
  planModalApplicationId = signal('');
  planModalAppRoles = signal<ApplicationRoleDto[]>([]);
  loadingPlanModalAppRoles = signal(false);
  loadingApps = signal(false);
  /** Khi sửa plan cũ chưa có assignedRoleIds: map CSV tên → id sau khi load roles. */
  private planRoleFallbackCsv: string | null = null;

  filteredPlans = computed(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.plans();
    return this.plans().filter(p =>
      p.name.toLowerCase().includes(q) ||
      (p.description || '').toLowerCase().includes(q) ||
      (p.assignedRole || '').toLowerCase().includes(q)
    );
  });

  appOptions = computed<DropdownOption[]>(() => {
    const list = this.apps();
    return [{ value: '', label: 'Gói dùng chung (global)' }, ...list.map(a => ({ value: a.id, label: a.name }))];
  });

  /** Role của application đã chọn trong form (+ role cũ không còn trong app). */
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
    this.refreshApplications();
    this.loadPlans();
  }

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

  refreshModalData() {
    this.refreshApplications();
  }

  planExtraRolesNotInModalApp(): string[] {
    const ids = new Set(this.planModalAppRoles().map(r => r.id));
    return (this.newPlan.assignedRoleIds ?? []).filter((rid: string) => !ids.has(rid));
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

  getApplicationName(appId: string | null | undefined): string {
    if (!appId) return 'Global';
    return this.apps().find(a => a.id === appId)?.name || appId;
  }

  loadPlans() {
    this.loading.set(true);
    this.subscriptionsService.getPlans(null).subscribe({
      next: res => this.plans.set(res.success && res.data ? res.data : []),
      error: () => {
        this.plans.set([]);
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }

  createPlan() {
    if (!this.newPlan.name?.trim()) return this.notify.error('Vui lòng nhập tên gói');
    const appId = this.newPlan.applicationId?.trim() ?? '';
    if (appId && !(this.newPlan.assignedRoleIds?.length ?? 0)) {
      return this.notify.error('Chọn ít nhất một role application cho gói');
    }

    this.savingCreate.set(true);
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
    const request$ = editingId
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

    request$.subscribe({
      next: res => {
        if (!res.success) {
          this.notify.error(res.message || (editingId ? 'Sửa gói thất bại' : 'Tạo gói thất bại'));
          this.savingCreate.set(false);
          return;
        }
        this.notify.success(editingId ? 'Đã cập nhật gói' : 'Đã tạo gói');
        this.resetCreateForm();
        this.closeCreate();
        this.loadPlans();
        this.savingCreate.set(false);
      },
      error: err => {
        this.notify.error(err.error?.message || (editingId ? 'Sửa gói thất bại' : 'Tạo gói thất bại'));
        this.savingCreate.set(false);
      }
    });
  }

  openCreate() {
    this.editingPlanId.set(null);
    this.resetCreateForm();
    this.refreshModalData();
    this.closingCreate.set(false);
    this.showCreateModal.set(true);
  }

  openEdit(plan: SubscriptionPlanCatalogDto) {
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
    this.closingCreate.set(false);
    this.showCreateModal.set(true);
  }

  closeCreate() {
    if (this.closingCreate()) return;
    this.closingCreate.set(true);
    setTimeout(() => {
      this.showCreateModal.set(false);
      this.closingCreate.set(false);
    }, 300);
  }

  private resetCreateForm() {
    this.planModalApplicationId.set('');
    this.planModalAppRoles.set([]);
    this.loadingPlanModalAppRoles.set(false);
    this.planRoleFallbackCsv = null;
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
      applicationId: '',
      assignedRoleIds: [],
      icon: null
    };
  }
}

