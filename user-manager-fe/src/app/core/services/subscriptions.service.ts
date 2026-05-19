import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse, SuccessResponse } from '../models/http.model';
import type {
  SubscriptionType,
  SubscriptionDto,
  SubscriptionPlanCatalogDto
} from '../models';

@Injectable({ providedIn: 'root' })
export class SubscriptionsService {
  private readonly api = `${environment.apiUrl}/api/subscriptions`;

  constructor(private http: HttpClient) { }

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
    subscriptionPlanId: string;
    subscriptionType: SubscriptionType;
    expiresAt: string;
    maxMembers: number;
  }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      subscriptionPlanId: data.subscriptionPlanId,
      subscriptionType: data.subscriptionType,
      expiresAt: data.expiresAt,
      maxMembers: data.maxMembers
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/activate`, payload);
  }

  upgrade(data: {
    userId?: string | null;
    applicationId: string;
    newSubscriptionPlanId: string;
    newType: SubscriptionType;
    expiresAt: string;
  }) {
    const payload: Record<string, unknown> = {
      applicationId: data.applicationId,
      newSubscriptionPlanId: data.newSubscriptionPlanId,
      newType: data.newType,
      expiresAt: data.expiresAt
    };
    if (data.userId) payload['userId'] = data.userId;
    return this.http.post<ApiResponse<SubscriptionDto>>(`${this.api}/upgrade`, payload);
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
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/cancel`, payload);
  }

  // ---------------- Plans Catalog ----------------

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
    maxApiCalls: number;
    features: string[];
    permissions: string[];
    isActive: boolean;
    displayOrder: number;
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
    maxApiCalls: number;
    features: string[];
    permissions: string[];
    isActive: boolean;
    displayOrder: number;
    icon: string | null;
  }) {
    return this.http.put<ApiResponse<SubscriptionPlanCatalogDto>>(`${this.api}/plans/${planId}`, data);
  }

  deletePlan(planId: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(`${this.api}/plans/${planId}`);
  }
}

