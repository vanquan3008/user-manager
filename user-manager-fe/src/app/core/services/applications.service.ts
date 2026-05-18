import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse, SuccessResponse } from '../models/http.model';
import type {
  ApplicationCapabilitiesDto,
  ApplicationCredentialItem,
  ApplicationListItem,
  ApplicationRoleDto,
  ApplicationUserDto
} from '../models';

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
  const clientId = str(raw['clientId'] ?? raw['ClientId']);
  const clientType = str(raw['clientType'] ?? raw['ClientType'], 'Public');
  const clientSecret = raw['clientSecret'] ?? raw['ClientSecret'];
  const lastClientSecretRotatedAt = raw['lastClientSecretRotatedAt'] ?? raw['LastClientSecretRotatedAt'];
  const odn = raw['ownerDisplayName'] ?? raw['OwnerDisplayName'];
  const logo = raw['logoUrl'] ?? raw['LogoUrl'];
  const backchannelLogoutUri = raw['backchannelLogoutUri'] ?? raw['BackchannelLogoutUri'];
  const allowedRedirectUris = raw['allowedRedirectUris'] ?? raw['AllowedRedirectUris'];
  const item: ApplicationListItem = {
    id,
    name,
    ...(clientId ? { clientId } : {}),
    ...(clientType ? { clientType } : {}),
    ...(desc ? { description: desc } : {}),
    ...(ownerId ? { ownerId } : {})
  };
  if (clientSecret !== undefined) item.clientSecret = clientSecret == null || String(clientSecret) === '' ? null : String(clientSecret);
  if (lastClientSecretRotatedAt !== undefined) item.lastClientSecretRotatedAt = lastClientSecretRotatedAt == null || String(lastClientSecretRotatedAt) === '' ? null : String(lastClientSecretRotatedAt);
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

function normalizeApplicationCredentialItem(raw: Record<string, unknown>): ApplicationCredentialItem {
  const lastRotated = raw['lastClientSecretRotatedAt'] ?? raw['LastClientSecretRotatedAt'];
  const clientSecret = raw['clientSecret'] ?? raw['ClientSecret'];
  return {
    applicationId: str(raw['applicationId'] ?? raw['ApplicationId']),
    applicationName: str(raw['applicationName'] ?? raw['ApplicationName']),
    clientId: str(raw['clientId'] ?? raw['ClientId']),
    clientType: str(raw['clientType'] ?? raw['ClientType'], 'Public'),
    clientSecret: clientSecret == null || String(clientSecret) === '' ? null : String(clientSecret),
    lastClientSecretRotatedAt: lastRotated == null || String(lastRotated) === '' ? null : String(lastRotated),
    allowedRedirectUris: str(raw['allowedRedirectUris'] ?? raw['AllowedRedirectUris']),
    backchannelLogoutUri: str(raw['backchannelLogoutUri'] ?? raw['BackchannelLogoutUri'])
  };
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

function toBooleanRecord(raw: unknown): Record<string, boolean> {
  if (!raw || typeof raw !== 'object' || Array.isArray(raw)) {
    return {};
  }

  return Object.entries(raw as Record<string, unknown>).reduce<Record<string, boolean>>((acc, [key, value]) => {
    acc[key] = value === true;
    return acc;
  }, {});
}

function normalizeApplicationCapabilitiesBody(body: unknown): ApplicationCapabilitiesDto {
  const source =
    body && typeof body === 'object' && !Array.isArray(body)
      ? (((body as Record<string, unknown>)['data'] ?? (body as Record<string, unknown>)['Data']) as unknown) ??
        body
      : body;

  const raw = source && typeof source === 'object' && !Array.isArray(source) ? (source as Record<string, unknown>) : {};
  const features = toBooleanRecord(raw['features'] ?? raw['Features']);
  const permissions = toBooleanRecord(raw['permissions'] ?? raw['Permissions']);

  return {
    applicationId: str(raw['applicationId'] ?? raw['ApplicationId']),
    features,
    permissions,
    enabledFeatures: Object.keys(features).filter(key => features[key]),
    enabledPermissions: Object.keys(permissions).filter(key => permissions[key])
  };
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
    clientType: string;
    subscriptionPlanId?: string;
    logoUrl?: string;
    backchannelLogoutUri?: string;
    allowedRedirectUris?: string;
  }) {
    return this.http.post<ApiResponse<ApplicationListItem>>(this.api, data);
  }

  deleteApplication(applicationId: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(`${this.api}/${applicationId}`);
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

  getApplicationCapabilities(applicationId: string) {
    return this.http
      .get<unknown>(`${this.api}/${applicationId}/me/capabilities`)
      .pipe(map(normalizeApplicationCapabilitiesBody));
  }

  getApplicationUsers(applicationId: string) {
    return this.http
      .get<ApiResponse<ApplicationUserDto[]>>(
        `${this.api}/${applicationId}/users`
      )
      .pipe(map(res => (res.success && res.data ? res.data : [])));
  }

  addUserToApplication(applicationId: string, userId: string, role: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/${applicationId}/users`, { userId, role });
  }

  removeUserFromApplication(applicationId: string, userId: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(`${this.api}/${applicationId}/users/${userId}`);
  }

  createApplicationRole(applicationId: string, data: { name: string; description?: string | null }) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/${applicationId}/roles`, {
      name: data.name,
      description: data.description ?? null
    });
  }

  updateApplicationRole(
    applicationId: string,
    roleName: string,
    data: { description?: string | null }
  ) {
    return this.http.put<ApiResponse<SuccessResponse>>(
      `${this.api}/${applicationId}/roles/${encodeURIComponent(roleName)}`,
      { description: data.description ?? null }
    );
  }

  deleteApplicationRole(applicationId: string, roleName: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(
      `${this.api}/${applicationId}/roles/${encodeURIComponent(roleName)}`
    );
  }

  getApplicationCredentials(applicationId: string) {
    return this.http
      .get<ApiResponse<ApplicationCredentialItem>>(`${this.api}/${applicationId}/credentials`)
      .pipe(map(res => ({
        ...res,
        data: res.data ? normalizeApplicationCredentialItem(res.data as unknown as Record<string, unknown>) : null
      })));
  }

  rotateApplicationClientSecret(applicationId: string) {
    return this.http
      .post<ApiResponse<ApplicationCredentialItem>>(`${this.api}/${applicationId}/credentials/rotate`, { confirm: true })
      .pipe(map(res => ({
        ...res,
        data: res.data ? normalizeApplicationCredentialItem(res.data as unknown as Record<string, unknown>) : null
      })));
  }
}
