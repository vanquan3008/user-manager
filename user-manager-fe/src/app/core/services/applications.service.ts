import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse } from '../models/api-response';
import type { ApplicationRoleDto } from '../models/api-response';

export interface ApplicationListItem {
  id: string;
  name: string;
  description?: string;
  ownerId?: string;
  ownerDisplayName?: string | null;
  logoUrl?: string;
  backchannelLogoutUri?: string;
  allowedRedirectUris?: string;
}

function str(v: unknown, fallback = ''): string {
  if (v == null) return fallback;
  const s = String(v);
  return s === 'undefined' || s === 'null' ? fallback : s;
}

/** Giống backend `GET /api/applications`: toàn bộ application đang active (không phân trang). */
function normalizeApplicationListItem(raw: Record<string, unknown>): ApplicationListItem {
  const id = str(raw['id'] ?? raw['Id']);
  const name = str(raw['name'] ?? raw['Name']);
  const desc = str(raw['description'] ?? raw['Description']);
  const ownerId = str(raw['ownerId'] ?? raw['OwnerId']);
  const odn = raw['ownerDisplayName'] ?? raw['OwnerDisplayName'];
  const logo = raw['logoUrl'] ?? raw['LogoUrl'];
  const backchannelLogoutUri = raw['backchannelLogoutUri'] ?? raw['BackchannelLogoutUri'];
  const allowedRedirectUris = raw['allowedRedirectUris'] ?? raw['AllowedRedirectUris'];
  const item: ApplicationListItem = {
    id,
    name,
    ...(desc ? { description: desc } : {}),
    ...(ownerId ? { ownerId } : {})
  };
  if (odn !== undefined && odn !== null) item.ownerDisplayName = odn === '' ? null : String(odn);
  if (logo != null && String(logo) !== '') item.logoUrl = String(logo);
  if (backchannelLogoutUri != null && String(backchannelLogoutUri) !== '') item.backchannelLogoutUri = String(backchannelLogoutUri);
  if (allowedRedirectUris != null && String(allowedRedirectUris) !== '') item.allowedRedirectUris = String(allowedRedirectUris);
  return item;
}

function parseApplicationsBody(body: unknown): ApplicationListItem[] {
  if (Array.isArray(body)) {
    return body
      .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
      .map(normalizeApplicationListItem)
      .filter((a) => a.id && a.name);
  }
  if (body && typeof body === 'object') {
    const o = body as Record<string, unknown>;
    const data = o['data'] ?? o['Data'];
    if (Array.isArray(data)) {
      return data
        .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
        .map(normalizeApplicationListItem)
        .filter((a) => a.id && a.name);
    }
  }
  return [];
}

function normalizeApplicationRoleDto(raw: Record<string, unknown>): ApplicationRoleDto {
  const u = raw['updatedAt'] ?? raw['UpdatedAt'];
  return {
    id: str(raw['id'] ?? raw['Id']),
    name: str(raw['name'] ?? raw['Name']),
    description: str(raw['description'] ?? raw['Description']),
    createdAt: str(raw['createdAt'] ?? raw['CreatedAt']),
    updatedAt: u == null || u === '' ? null : str(u)
  };
}

function parseApplicationRolesBody(body: unknown): ApplicationRoleDto[] {
  if (Array.isArray(body)) {
    return body
      .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
      .map(normalizeApplicationRoleDto)
      .filter((r) => r.id || r.name);
  }
  if (body && typeof body === 'object') {
    const o = body as Record<string, unknown>;
    const data = o['data'] ?? o['Data'];
    if (Array.isArray(data)) {
      return data
        .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
        .map(normalizeApplicationRoleDto)
        .filter((r) => r.id || r.name);
    }
  }
  return [];
}

@Injectable({ providedIn: 'root' })
export class ApplicationsService {
  private readonly api = `${environment.apiUrl}/api/applications`;

  constructor(private http: HttpClient) {}

  getApplications() {
    return this.http.get<unknown>(this.api).pipe(map(parseApplicationsBody));
  }

  getApplication(applicationId: string) {
    return this.http.get<ApiResponse<ApplicationListItem>>(`${this.api}/${applicationId}`);
  }

  createApplication(data: {
    name: string;
    description?: string;
    ownerId: string;
    defaultLicensePackageId?: string;
    subscriptionPlanId?: string;
    logoUrl?: string;
    backchannelLogoutUri?: string;
    allowedRedirectUris?: string;
  }) {
    return this.http.post<ApiResponse<ApplicationListItem>>(this.api, data);
  }

  deleteApplication(applicationId: string) {
    return this.http.delete<ApiResponse<unknown>>(`${this.api}/${applicationId}`);
  }

  updateApplicationLogo(applicationId: string, logoUrl?: string) {
    return this.http.put<ApiResponse<ApplicationListItem>>(
      `${this.api}/${applicationId}/logo`,
      { logoUrl: logoUrl ?? null }
    );
  }

  getApplicationRoles(applicationId: string) {
    return this.http.get<unknown>(`${this.api}/${applicationId}/roles`).pipe(map(parseApplicationRolesBody));
  }

  getApplicationUsers(applicationId: string) {
    return this.http
      .get<ApiResponse<Array<{ userId: string; role: string; email?: string; userName?: string }>>>(
        `${this.api}/${applicationId}/users`
      )
      .pipe(map(res => (res.success && res.data ? res.data : [])));
  }

  addUserToApplication(applicationId: string, userId: string, role: string) {
    return this.http.post<ApiResponse<unknown>>(`${this.api}/${applicationId}/users`, { userId, role });
  }

  createApplicationRole(applicationId: string, data: { name: string; description?: string | null }) {
    return this.http.post<ApiResponse<unknown>>(`${this.api}/${applicationId}/roles`, {
      name: data.name,
      description: data.description ?? null
    });
  }
}
