import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse } from '../models/api-response';
import type {
  SubscriptionDto,
  SubscriptionPlanCatalogDto,
  LicensePackageCatalogDto
} from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class SubscriptionsService {
  private readonly api = `${environment.apiUrl}/api/subscriptions`;

  constructor(private http: HttpClient) {}

  getActive(applicationId: string, userId?: string | null) {
    let params = new HttpParams().set('applicationId', applicationId);
    if (userId) params = params.set('userId', userId);
    return this.http.get<ApiResponse<SubscriptionDto | null>>(`${this.api}/active`, { params });
  }

  getHistory(applicationId: string, userId?: string | null) {
    let params = new HttpParams().set('applicationId', applicationId);
    if (userId) params = params.set('userId', userId);
    return this.http.get<ApiResponse<SubscriptionDto[]>>(`${this.api}/history`, { params }).pipe(
      map(res => (res.success && res.data ? res.data : []))
    );
  }

  activate(data: {
    userId?: string | null;
    applicationId: string;
    licensePackageId: string;
    subscriptionType: 'Free' | 'Premium' | 'Family' | 'Student';
    expiresAt: string;
    maxMembers: number;
  }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      licensePackageId: data.licensePackageId,
      subscriptionType: data.subscriptionType,
      expiresAt: data.expiresAt,
      maxMembers: data.maxMembers
    };
    if (data.userId) payload['userId'] = data.userId;
    // Note: hitting /activate-package specifically as FE now prioritizes packets
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/activate-package`, payload);
  }

  upgrade(data: {
    userId?: string | null;
    applicationId: string;
    newLicensePackageId: string;
    newType: 'Free' | 'Premium' | 'Family' | 'Student';
    expiresAt: string;
  }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      newLicensePackageId: data.newLicensePackageId,
      newType: data.newType,
      expiresAt: data.expiresAt
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/upgrade-package`, payload);
  }

  extend(data: { userId?: string | null; applicationId: string; newExpiresAt: string }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      newExpiresAt: data.newExpiresAt
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/extend`, payload);
  }

  cancel(data: { userId?: string | null; applicationId: string }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<unknown>>(`${this.api}/cancel`, payload);
  }

  // ---------------- Catalog: plans / packages ----------------

  getPlans(applicationId?: string | null) {
    let params = new HttpParams();
    if (applicationId) params = params.set('applicationId', applicationId);
    return this.http.get<ApiResponse<SubscriptionPlanCatalogDto[]>>(`${this.api}/plans`, { params });
  }

  createPlan(data: {
    applicationId: string | null;
    name: string;
    description: string;
    monthlyPrice: number;
    yearlyPrice: number;
    maxUsers: number;
    /** Không giới hạn / không nhập: luôn gửi -1 */
    maxApiCalls: number;
    includeAnalytics: boolean;
    includeSupport: boolean;
    includeAdvancedSecurity: boolean;
    includeCustomDomain: boolean;
    isActive: boolean;
    displayOrder: number;
    assignedRoleIds: string[];
    icon: string | null;
  }) {
    return this.http.post<ApiResponse<SubscriptionPlanCatalogDto>>(`${this.api}/plans`, data);
  }

  updatePlan(planId: string, data: {
    applicationId: string | null;
    name: string;
    description: string;
    monthlyPrice: number;
    yearlyPrice: number;
    maxUsers: number;
    /** Không giới hạn / không nhập: luôn gửi -1 */
    maxApiCalls: number;
    includeAnalytics: boolean;
    includeSupport: boolean;
    includeAdvancedSecurity: boolean;
    includeCustomDomain: boolean;
    isActive: boolean;
    displayOrder: number;
    assignedRoleIds: string[];
    icon: string | null;
  }) {
    return this.http.put<ApiResponse<SubscriptionPlanCatalogDto>>(`${this.api}/plans/${planId}`, data);
  }

  getPackages(applicationId: string, subscriptionPlanId?: string | null) {
    let params = new HttpParams().set('applicationId', applicationId);
    if (subscriptionPlanId) params = params.set('subscriptionPlanId', subscriptionPlanId);
    return this.http.get<ApiResponse<LicensePackageCatalogDto[]>>(`${this.api}/packages`, { params });
  }

  createPackage(data: {
    applicationId: string;
    subscriptionPlanId: string;
    name: string;
    description: string | null;
    features: string[];
    seatLimit: number;
    isActive: boolean;
    displayOrder: number;
    assignedRole: string;
  }) {
    return this.http.post<ApiResponse<LicensePackageCatalogDto>>(`${this.api}/packages`, data);
  }

  updatePackage(packageId: string, data: {
    applicationId: string;
    name: string;
    description: string | null;
    features: string[];
    seatLimit: number;
    isActive: boolean;
    displayOrder: number;
    assignedRole: string;
  }) {
    return this.http.put<ApiResponse<LicensePackageCatalogDto>>(`${this.api}/packages/${packageId}`, data);
  }

  // Activate using a license package (packet)
  activatePackage(data: {
    userId?: string | null;
    applicationId: string;
    licensePackageId: string;
    subscriptionType: 'Free' | 'Premium' | 'Family' | 'Student';
    expiresAt: string | null;
    maxMembers: number;
  }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      licensePackageId: data.licensePackageId,
      subscriptionType: data.subscriptionType,
      expiresAt: data.expiresAt,
      maxMembers: data.maxMembers
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/activate-package`, payload);
  }

  addMember(data: { subscriptionId: string; userId: string; roleTypeId: string }) {
    return this.http.post<ApiResponse<unknown>>(`${this.api}/members`, data);
  }

  removeMember(subscriptionId: string, userId: string) {
    const params = new HttpParams().set('subscriptionId', subscriptionId).set('userId', userId);
    return this.http.delete<ApiResponse<unknown>>(`${this.api}/members`, { params });
  }
}

