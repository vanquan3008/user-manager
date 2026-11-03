import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse, PagedResponse, SuccessResponse } from '../models/http.model';
import type {
  AuditLogEntry,
  AuditLogFilter,
  DashboardSummary,
  MfaSetup,
  MfaStatus,
  SecuritySettings,
  SessionInfo,
  SigningKeyInfo
} from '../models/security.model';

/** Self-service account security, audit trail, dashboard metrics and platform key management. */
@Injectable({ providedIn: 'root' })
export class SecurityService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  // --- My account
  mySessions() {
    return this.http.get<ApiResponse<SessionInfo[]>>(`${this.api}/api/account/sessions`).pipe(map(r => r.data ?? []));
  }
  revokeMySession(id: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(`${this.api}/api/account/sessions/${id}`);
  }
  revokeOtherSessions() {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/api/account/sessions/revoke-others`, {});
  }
  mfaStatus() {
    return this.http.get<ApiResponse<MfaStatus>>(`${this.api}/api/account/mfa`).pipe(map(r => r.data!));
  }
  beginMfaSetup() {
    return this.http.post<ApiResponse<MfaSetup>>(`${this.api}/api/account/mfa/setup`, {}).pipe(map(r => r.data!));
  }
  enableMfa(code: string) {
    return this.http.post<ApiResponse<{ recoveryCodes: string[] }>>(`${this.api}/api/account/mfa/enable`, { code }).pipe(map(r => r.data!.recoveryCodes));
  }
  disableMfa(password: string, code: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/api/account/mfa/disable`, { password, code });
  }
  regenerateRecoveryCodes(code: string) {
    return this.http.post<ApiResponse<{ recoveryCodes: string[] }>>(`${this.api}/api/account/mfa/recovery-codes`, { code }).pipe(map(r => r.data!.recoveryCodes));
  }
  myActivity(page = 1, pageSize = 20) {
    return this.http.get<PagedResponse<AuditLogEntry>>(`${this.api}/api/account/activity`, { params: { page, pageSize } });
  }

  // --- Administration
  userSessions(userId: string) {
    return this.http.get<ApiResponse<SessionInfo[]>>(`${this.api}/api/users/${userId}/sessions`).pipe(map(r => r.data ?? []));
  }
  revokeUserSessions(userId: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/api/users/${userId}/sessions/revoke`, {});
  }
  userMfa(userId: string) {
    return this.http.get<ApiResponse<MfaStatus>>(`${this.api}/api/users/${userId}/mfa`).pipe(map(r => r.data!));
  }
  resetUserMfa(userId: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/api/users/${userId}/mfa/reset`, {});
  }

  auditLogs(filter: AuditLogFilter) {
    return this.http.get<PagedResponse<AuditLogEntry>>(`${this.api}/api/audit-logs`, { params: this.auditParams(filter) });
  }
  exportAuditLogs(filter: AuditLogFilter) {
    return this.http.get(`${this.api}/api/audit-logs/export`, { params: this.auditParams(filter), responseType: 'blob' });
  }

  dashboard() {
    return this.http.get<ApiResponse<DashboardSummary>>(`${this.api}/api/dashboard/summary`).pipe(map(r => r.data!));
  }

  signingKeys() {
    return this.http.get<ApiResponse<SigningKeyInfo[]>>(`${this.api}/api/system/signing-keys`).pipe(map(r => r.data ?? []));
  }
  rotateSigningKey() {
    return this.http.post<ApiResponse<SigningKeyInfo>>(`${this.api}/api/system/signing-keys/rotate`, {});
  }
  revokeSigningKey(keyId: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/api/system/signing-keys/${encodeURIComponent(keyId)}/revoke`, {});
  }
  securitySettings() {
    return this.http.get<ApiResponse<SecuritySettings>>(`${this.api}/api/system/security-settings`).pipe(map(r => r.data!));
  }

  private auditParams(f: AuditLogFilter): HttpParams {
    let p = new HttpParams().set('page', f.page).set('pageSize', f.pageSize);
    if (f.search?.trim()) p = p.set('search', f.search.trim());
    if (f.category) p = p.set('category', f.category);
    if (f.succeeded !== null && f.succeeded !== undefined) p = p.set('succeeded', f.succeeded);
    if (f.from) p = p.set('from', new Date(f.from).toISOString());
    if (f.to) p = p.set('to', new Date(f.to + 'T23:59:59').toISOString());
    if (f.userId) p = p.set('userId', f.userId);
    return p;
  }
}
