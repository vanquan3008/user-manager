import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { ApiResponse, PagedResponse, SuccessResponse } from '../models/http.model';
import type {
  IdentityUser,
  UserWithRoles,
  SsoUserApplication
} from '../models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly api = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getUsers(page = 1, pageSize = 20, search?: string) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<PagedResponse<IdentityUser>>(this.api, { params });
  }

  getUser(id: string) {
    return this.http.get<ApiResponse<UserWithRoles>>(`${this.api}/${id}`);
  }

  /** Ban (ManageUsers / Admin hoặc app admin có users.manage) */
  banUser(userId: string, reason?: string | null) {
    return this.http.post<ApiResponse<{ success: boolean; message: string }>>(`${this.api}/${userId}/ban`, {
      reason: reason ?? null
    });
  }

  unbanUser(userId: string) {
    return this.http.post<ApiResponse<{ success: boolean; message: string }>>(`${this.api}/${userId}/unban`, {});
  }

  addRole(userId: string, role: string) {
    return this.http.post<ApiResponse<{ success: boolean; message?: string }>>(
      `${this.api}/${userId}/roles/upgrade`,
      { role }
    );
  }

  removeRole(userId: string, roleName: string) {
    return this.http.delete<ApiResponse<{ success: boolean }>>(
      `${this.api}/${userId}/roles/${encodeURIComponent(roleName)}`
    );
  }

  /** Tạo SSO user (SuperAdmin only) */
  createSsoUser(data: {
    email: string;
    password: string;
    username: string;
    firstName: string;
    lastName: string;
    /** ISO date `YYYY-MM-DD` → API `DateOnly` */
    dateOfBirth: string;
  }) {
    return this.http.post<ApiResponse<IdentityUser>>(`${this.api}/sso`, data);
  }

  /** Gán SSO user vào application (Admin only) */
  grantSsoUserApplication(ssoUserId: string, applicationId: string, role: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/sso/${ssoUserId}/applications`, {
      applicationId,
      role
    });
  }

  /** Thu hồi quyền SSO user khỏi application (Admin only) */
  revokeSsoUserApplication(ssoUserId: string, applicationId: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(
      `${this.api}/sso/${ssoUserId}/applications/${applicationId}`
    );
  }

  /** Lấy danh sách applications của SSO user (nếu backend hỗ trợ) */
  getSsoUserApplications(ssoUserId: string) {
    return this.http.get<ApiResponse<SsoUserApplication[]>>(
      `${this.api}/sso/${ssoUserId}/applications`
    );
  }
}
