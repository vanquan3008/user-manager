import { Component, signal, inject, OnInit, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SubscriptionsService } from '../../core/services/subscriptions.service';
import { ApplicationsService } from '../../core/services/applications.service';
import type { ApplicationListItem } from '../../core/services/applications.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { DropdownComponent, type DropdownOption } from '../../shared/components/dropdown/dropdown.component';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import type {
  ApplicationRoleDto,
  LicensePackageCatalogDto,
  SubscriptionDto,
  SubscriptionMemberDto,
  SubscriptionPlanCatalogDto
} from '../../core/models/api-response';
import { assignedRolesToCsv, DEFAULT_ASSIGNED_ROLE } from '../../core/utils/assigned-roles';
import { parsePlanLimitField } from '../../core/utils/plan-limit-fields';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../core/utils/http-error.utils';

/** User gắn với application (dropdown), từ GET /applications/{id}/users */
export interface AppMemberRow {
  id: string;
  email?: string;
  userName?: string;
  role?: string;
}

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, DrawerComponent, FormFieldComponent, DrawerFooterActionsComponent, LoadingStateComponent],
  templateUrl: './subscriptions.component.html'
})
export class SubscriptionsComponent implements OnInit {
  private subsService = inject(SubscriptionsService);
  private applicationsService = inject(ApplicationsService);
  private notify = inject(NotificationService);
  private confirm = inject(ConfirmService);
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);

  userId = signal('');
  applicationId = signal('');
  /** Thành viên của application đang chọn */
  appMembers = signal<AppMemberRow[]>([]);
  apps = signal<ApplicationListItem[]>([]);
  loadingUsers = signal(false);
  active = signal<SubscriptionDto | null>(null);
  history = signal<SubscriptionDto[]>([]);
  loading = signal(false);
  showActivateModal = signal(false);
  closingActivate = signal(false);
  saving = signal(false);

  // Catalog management (subplans / packets)
  loadingCatalog = signal(false);
  plans = signal<SubscriptionPlanCatalogDto[]>([]);
  packages = signal<LicensePackageCatalogDto[]>([]);
  appRoles = signal<ApplicationRoleDto[]>([]);

  activateSource = signal<'plan' | 'package'>('plan');

  activateForm = {
    licensePackageId: '', // Now mandatory
    subscriptionType: 'Premium' as 'Free' | 'Premium' | 'Family' | 'Student',
    expiresAt: '',
    maxMembers: 1
  };

  userOptions = computed<DropdownOption[]>(() => {
    const appId = this.applicationId().trim();
    const list = this.appMembers();
    let ph: string;
    if (!appId) {
      ph = '— Chọn application trước —';
    } else if (this.loadingUsers()) {
      ph = this.translate.t().subsLoadingUsers;
    } else if (list.length === 0) {
      ph = '— Không có user trong app —';
    } else {
      ph = this.translate.t().selectUser;
    }
    const placeholder: DropdownOption = { value: '', label: ph };
    return [
      placeholder,
      ...list.map((u) => ({
        value: u.id,
        label: [u.email || u.userName, u.role].filter(Boolean).join(' · ') || u.id
      }))
    ];
  });

  appOptions = computed<DropdownOption[]>(() => {
    const list = this.apps();
    return [{ value: '', label: this.translate.t().selectApplication }, ...list.map(a => ({ value: a.id, label: a.name }))];
  });

  selectedAppName = computed(() => {
    const id = this.applicationId().trim();
    if (!id) return '';
    return this.apps().find((a) => a.id === id)?.name ?? '';
  });

  selectedUserSummary = computed(() => {
    const id = this.userId().trim();
    if (!id) return '';
    const m = this.appMembers().find((u) => u.id === id);
    if (!m) return id;
    const main = m.email || m.userName || id;
    return m.role ? `${main} · ${m.role}` : main;
  });

  step1Complete = computed(() => !!this.applicationId().trim());
  step2Complete = computed(() => !!this.userId().trim() && !!this.applicationId().trim());

  // Create forms (catalog)
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
    icon: null as string | null
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

  constructor() {
    effect(() => {
      const uid = this.userId().trim();
      const appId = this.applicationId().trim();
      if (uid && appId) {
        this.loadSubscriptions();
        this.loadCatalog(appId);
      } else {
        this.active.set(null);
        this.history.set([]);
        this.plans.set([]);
        this.packages.set([]);
        this.appRoles.set([]);
      }
    });
  }

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: this.translate.t().subscriptions,
      description: this.translate.t().subsPageDesc
    });
    this.applicationsService.getApplications().subscribe({
      next: (list) => this.apps.set(list),
      error: () => this.apps.set([])
    });
  }

  onApplicationIdChange(nextAppId: string) {
    this.userId.set('');
    this.applicationId.set(nextAppId);
    const id = nextAppId.trim();
    if (!id) {
      this.appMembers.set([]);
      return;
    }
    this.loadAppMembers(id);
  }

  loadAppMembers(applicationId: string) {
    this.loadingUsers.set(true);
    this.applicationsService.getApplicationUsers(applicationId).subscribe({
      next: (list) => {
        this.appMembers.set(
          list.map((m) => ({
            id: m.userId,
            email: m.email,
            userName: m.userName,
            role: m.role
          }))
        );
        this.loadingUsers.set(false);
      },
      error: () => {
        this.notify.error('Không thể tải user của application');
        this.appMembers.set([]);
        this.loadingUsers.set(false);
      }
    });
  }

  loadSubscriptions() {
    const uid = this.userId().trim();
    const appId = this.applicationId().trim();
    if (!uid || !appId) {
      this.notify.error('Vui lòng chọn Application và User');
      return;
    }
    this.loading.set(true);
    this.subsService.getActive(appId, uid).subscribe({
      next: (res) => {
        this.active.set(res.success && res.data ? res.data : null);
        this.loading.set(false);
      },
      error: () => {
        this.active.set(null);
        this.loading.set(false);
      }
    });
    this.subsService.getHistory(appId, uid).subscribe({
      next: (list) => this.history.set(list),
      error: () => this.history.set([])
    });
  }

  loadCatalog(appId: string) {
    if (!appId) return;
    this.loadingCatalog.set(true);

    const finish = (() => {
      let done = 0;
      return () => {
        done++;
        if (done >= 3) this.loadingCatalog.set(false);
      };
    })();

    this.subsService.getPlans(appId).subscribe({
      next: (res) => {
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

    this.subsService.getPackages(appId).subscribe({
      next: (res) => this.packages.set(res.success && res.data ? res.data : []),
      error: () => {
        this.packages.set([]);
        finish();
      },
      complete: finish
    });

    this.applicationsService.getApplicationRoles(appId).subscribe({
      next: (roles) => {
        this.appRoles.set(roles);
        const firstRole = roles.length > 0 ? roles[0].name : '';
        const firstId = roles.length > 0 ? roles[0].id : '';
        if (!this.newPlan.assignedRoleIds?.length) {
          this.newPlan.assignedRoleIds = firstId ? [firstId] : [];
        }
        if (!this.newPackage.assignedRoles?.length) {
          this.newPackage.assignedRoles = firstRole ? [firstRole] : [DEFAULT_ASSIGNED_ROLE];
        }
        if (this.activateForm.licensePackageId === '' && this.packages().length > 0) {
          this.activateForm.licensePackageId = this.packages()[0]?.id ?? '';
        }
      },
      error: () => {
        this.appRoles.set([]);
        finish();
      },
      complete: finish
    });
  }

  openActivateModal() {
    const d = new Date();
    d.setFullYear(d.getFullYear() + 1);
    const packageId = this.packages()[0]?.id ?? '';
    this.activateForm = {
      licensePackageId: packageId,
      subscriptionType: 'Premium',
      expiresAt: d.toISOString().slice(0, 10),
      maxMembers: 1
    };
    this.closingActivate.set(false);
    this.showActivateModal.set(true);
  }

  onActivateSourceChange(source: 'plan' | 'package') {
    this.activateSource.set(source);
    // Hierarchy: Subscription activation now requires a LicensePackage (packet).
    // If user switches source, we still keep or reset the packageId.
    if (!this.activateForm.licensePackageId && this.packages().length > 0) {
      this.activateForm.licensePackageId = this.packages()[0]?.id ?? '';
    }
  }

  closeActivateModal() {
    if (this.closingActivate()) return;
    this.closingActivate.set(true);
    setTimeout(() => {
      this.showActivateModal.set(false);
      this.closingActivate.set(false);
    }, 300);
  }

  doActivate() {
    const uid = this.userId().trim();
    const appId = this.applicationId().trim();
    if (!uid || !appId) return;
    const f = this.activateForm;
    if (!f.licensePackageId) {
      this.notify.error('Vui lòng chọn Package');
      return;
    }
    if (!Number.isFinite(Number(f.maxMembers)) || Number(f.maxMembers) <= 0) {
      this.notify.error('MaxMembers phải lớn hơn 0');
      return;
    }
    
    const expiresAt = new Date(f.expiresAt).toISOString();
    this.saving.set(true);
    
    this.subsService.activate({
      userId: uid,
      applicationId: appId,
      licensePackageId: f.licensePackageId,
      subscriptionType: f.subscriptionType,
      expiresAt,
      maxMembers: Math.trunc(Number(f.maxMembers))
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Kích hoạt subscription thành công');
          this.closeActivateModal();
          this.loadSubscriptions();
        } else {
          this.notify.error(res.message || 'Kích hoạt thất bại');
        }
        this.saving.set(false);
      },
      error: (err) => {
        this.notify.error(getHttpErrorMessage(err, 'Kích hoạt thất bại', UNREACHABLE_SERVER_MESSAGE_VI));
        this.saving.set(false);
      }
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
      this.notify.error('Chọn ít nhất một role application');
      return;
    }
    if (!this.validatePlanConstraints()) {
      return;
    }

    this.subsService.createPlan({
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
      icon: this.newPlan.icon
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã tạo subplan');
          this.resetPlanForm();
          this.loadCatalog(appId);
        } else {
          this.notify.error(res.message || 'Tạo subplan thất bại');
        }
      },
      error: (err) => this.notify.error(getHttpErrorMessage(err, 'Tạo subplan thất bại', UNREACHABLE_SERVER_MESSAGE_VI))
    });
  }

  private resetPlanForm() {
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
      assignedRoleIds: [],
      icon: null
    };
  }

  createPackage() {
    const appId = this.applicationId().trim();
    if (!appId) return;
    const planId = (this.newPackage.subscriptionPlanId || '').trim();
    if (!planId) return this.notify.error('Tạo hoặc chọn gói (subplan) trước khi tạo packet');
    if (!this.newPackage.name?.trim()) return this.notify.error('Vui lòng nhập tên packet');
    if (!this.validatePackageConstraints()) return;
    
    const features: string[] = [];
    if (this.newPackage.featureAnalytics) features.push('Analytics');
    if (this.newPackage.featureSupport) features.push('Support');
    if (this.newPackage.featureAdvancedSecurity) features.push('AdvancedSecurity');
    if (this.newPackage.featureCustomDomain) features.push('CustomDomain');

    this.subsService.createPackage({
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
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã tạo packet');
          this.resetPackageForm();
          this.loadCatalog(appId);
        } else {
          this.notify.error(res.message || 'Tạo packet thất bại');
        }
      },
      error: (err) => this.notify.error(getHttpErrorMessage(err, 'Tạo packet thất bại', UNREACHABLE_SERVER_MESSAGE_VI))
    });
  }

  private validatePlanConstraints(): boolean {
    const monthlyPrice = Number(this.newPlan.monthlyPrice);
    const yearlyPrice = Number(this.newPlan.yearlyPrice);
    if (!Number.isFinite(monthlyPrice) || monthlyPrice < 0) {
      this.notify.error('Giá tháng phải lớn hơn hoặc bằng 0');
      return false;
    }
    if (!Number.isFinite(yearlyPrice) || yearlyPrice < 0) {
      this.notify.error('Giá năm phải lớn hơn hoặc bằng 0');
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

  /** Quản lý thành viên (Seats) */
  showMemberModal = signal(false);
  addingMember = signal(false);
  memberForm = {
    userId: '',
    roleTypeId: '00000000-0000-0000-0000-000000000000' // Placeholder for member
  };

  openAddMember() {
    this.memberForm.userId = '';
    this.showMemberModal.set(true);
  }

  addMember() {
    const sub = this.active();
    if (!sub || !this.memberForm.userId) return;

    this.addingMember.set(true);
    this.subsService.addMember({
      subscriptionId: sub.id,
      userId: this.memberForm.userId,
      roleTypeId: this.memberForm.roleTypeId
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã thêm thành viên');
          this.showMemberModal.set(false);
          this.loadSubscriptions(); // Refresh to get member list
        } else {
          this.notify.error(res.message || 'Thêm thành viên thất bại');
        }
        this.addingMember.set(false);
      },
      error: (err) => {
        this.notify.error(err.error?.message || 'Thêm thành viên thất bại');
        this.addingMember.set(false);
      }
    });
  }

  async removeMember(m: SubscriptionMemberDto) {
    const sub = this.active();
    if (!sub) return;

    const ok = await this.confirm.confirm({
      title: 'Gỡ thành viên',
      message: `Bạn có chắc muốn gỡ ${m.userName} khỏi gói dịch vụ này?`,
      confirmText: 'Gỡ bỏ',
      variant: 'danger'
    });
    if (!ok) return;

    this.subsService.removeMember(sub.id, m.userId).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã gỡ thành viên');
          this.loadSubscriptions();
        } else {
          this.notify.error(res.message || 'Gỡ thành viên thất bại');
        }
      }
    });
  }

  private resetPackageForm() {
    this.newPackage = {
      subscriptionPlanId: this.plans()[0]?.id ?? '',
      name: '',
      assignedRoles: [DEFAULT_ASSIGNED_ROLE],
      seatLimit: -1,
      isActive: true,
      displayOrder: 0,
      featureAnalytics: false,
      featureSupport: false,
      featureAdvancedSecurity: false,
      featureCustomDomain: false
    };
  }

  async doCancel() {
    const uid = this.userId().trim();
    const appId = this.applicationId().trim();
    if (!uid || !appId) return;
    const ok = await this.confirm.confirm({
      title: 'Hủy subscription',
      message: 'Bạn chắc chắn muốn hủy subscription này?',
      confirmText: 'Hủy subscription',
      cancelText: 'Quay lại',
      variant: 'danger'
    });
    if (!ok) return;
    this.saving.set(true);
    this.subsService.cancel({ userId: uid, applicationId: appId }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Đã hủy subscription');
          this.loadSubscriptions();
        } else {
          this.notify.error(res.message || 'Hủy thất bại');
        }
        this.saving.set(false);
      },
      error: (err) => {
        this.notify.error(err.error?.message || 'Hủy thất bại');
        this.saving.set(false);
      }
    });
  }
}
