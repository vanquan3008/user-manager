import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApplicationsService, type ApplicationListItem } from '../../core/services/applications.service';
import { RolesService } from '../../core/services/roles.service';
import { SubscriptionsService } from '../../core/services/subscriptions.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { DropdownComponent, type DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import {
  MultiSelectDropdownComponent,
  type MultiSelectOption
} from '../../shared/components/multi-select-dropdown/multi-select-dropdown.component';
import type { ApplicationRoleDto, LicensePackageCatalogDto, RoleDto, SubscriptionPlanCatalogDto } from '../../core/models/api-response';
import { assignedRolesToCsv, DEFAULT_ASSIGNED_ROLE } from '../../core/utils/assigned-roles';

@Component({
  selector: 'app-application-package-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownComponent,
    FormFieldComponent,
    LoadingStateComponent,
    MultiSelectDropdownComponent
  ],
  templateUrl: './application-package-catalog.component.html'
})
export class ApplicationPackageCatalogComponent implements OnInit {
  private applicationsService = inject(ApplicationsService);
  private rolesService = inject(RolesService);
  private subscriptionsService = inject(SubscriptionsService);
  private notify = inject(NotificationService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);

  applicationId = signal('');
  apps = signal<ApplicationListItem[]>([]);
  appRoles = signal<ApplicationRoleDto[]>([]);
  globalRoles = signal<RoleDto[]>([]);
  selectedGlobalRoleId = signal('');
  loadingGlobalRoles = signal(false);
  loadingApps = signal(false);
  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  packages = signal<LicensePackageCatalogDto[]>([]);
  loading = signal(false);
  addingRole = signal(false);

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

  appOptions = computed<DropdownOption[]>(() => {
    const list = this.apps();
    return [{ value: '', label: this.translate.t().selectApplication }, ...list.map(a => ({ value: a.id, label: a.name }))];
  });

  globalRoleOptions = computed<DropdownOption[]>(() => [
    { value: '', label: 'Chọn global role' },
    ...this.globalRoles().map(r => ({ value: r.id, label: r.name }))
  ]);

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

  ngOnInit(): void {
    this.refreshApplications();
    this.loadGlobalRoles();
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

  loadGlobalRoles() {
    this.loadingGlobalRoles.set(true);
    this.rolesService.getRoles().subscribe({
      next: roles => {
        this.globalRoles.set(roles);
        this.loadingGlobalRoles.set(false);
      },
      error: () => {
        this.globalRoles.set([]);
        this.loadingGlobalRoles.set(false);
      }
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
    this.loadPackages();
  }

  loadPackages() {
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
        if (!this.newPackage.assignedRoles?.length && roles[0]?.name) {
          this.newPackage.assignedRoles = [roles[0].name];
        }
      },
      error: () => {
        this.appRoles.set([]);
        finish();
      },
      complete: finish
    });
  }

  licenseExtraRolesNotInApp(): string[] {
    const g = new Set(this.appRoles().map(r => r.name));
    return (this.newPackage.assignedRoles ?? []).filter((n: string) => !g.has(n));
  }

  planNameForLicense(p: LicensePackageCatalogDto): string {
    if (!p.subscriptionPlanId) return '—';
    return this.plans().find(pl => pl.id === p.subscriptionPlanId)?.name ?? '—';
  }

  createPackage() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    const planId = (this.newPackage.subscriptionPlanId || '').trim();
    if (!planId) return this.notify.error('Chọn gói (subplan) chứa packet');
    if (!this.newPackage.name?.trim()) return this.notify.error('Vui lòng nhập tên packet');
    if (!this.newPackage.assignedRoles?.length) {
      this.newPackage.assignedRoles = [DEFAULT_ASSIGNED_ROLE];
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
      features: features, // Using strongly-typed array
      seatLimit: Number(this.newPackage.seatLimit),
      isActive: !!this.newPackage.isActive,
      displayOrder: Number(this.newPackage.displayOrder),
      assignedRole: assignedRolesToCsv(this.newPackage.assignedRoles)
    }).subscribe({
      next: res => {
        if (!res.success) return this.notify.error(res.message || 'Tạo packet thất bại');
        this.notify.success('Đã tạo packet');
        this.newPackage.name = '';
        this.newPackage.subscriptionPlanId = this.plans()[0]?.id ?? '';
        this.loadPackages();
      },
      error: err => this.notify.error(err.error?.message || 'Tạo packet thất bại')
    });
  }

  addRoleToApplication() {
    const appId = this.applicationId().trim();
    if (!appId) return;

    const selectedRole = this.globalRoles().find(r => r.id === this.selectedGlobalRoleId());
    if (!selectedRole) {
      this.notify.error('Vui lòng chọn global role');
      return;
    }
    const roleName = selectedRole.name.trim();

    this.addingRole.set(true);
    this.applicationsService.createApplicationRole(appId, {
      name: roleName,
      description: selectedRole.description?.trim() || null
    }).subscribe({
      next: (res) => {
        if (!res.success) {
          this.notify.error(res.message || 'Thêm global role vào application thất bại');
          this.addingRole.set(false);
          return;
        }
        this.notify.success(`Đã thêm global role '${roleName}' vào application`);
        this.loadPackages();
        if (!this.newPackage.assignedRoles) this.newPackage.assignedRoles = [];
        if (!this.newPackage.assignedRoles.includes(roleName)) {
          this.newPackage.assignedRoles.push(roleName);
        }
        this.addingRole.set(false);
      },
      error: (err) => {
        this.notify.error(err.error?.message || 'Thêm global role vào application thất bại');
        this.addingRole.set(false);
      }
    });
  }
}

