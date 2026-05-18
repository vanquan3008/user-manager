import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse, SuccessResponse } from '../models/http.model';
import type { RoleDto } from '../models';

function str(v: unknown, fallback = ''): string {
  if (v == null) return fallback;
  const s = String(v);
  return s === 'undefined' || s === 'null' ? fallback : s;
}

/** Chuẩn hóa từ JSON camelCase hoặc PascalCase (proxy / cấu hình serializer). */
function normalizeRoleDto(raw: Record<string, unknown>): RoleDto {
  const id = str(raw['id'] ?? raw['Id']);
  const name = str(raw['name'] ?? raw['Name']);
  const description = str(raw['description'] ?? raw['Description']);
  const isSystemRole = Boolean(raw['isSystemRole'] ?? raw['IsSystemRole']);
  const permissions = str(raw['permissions'] ?? raw['Permissions']);
  const createdAt = str(raw['createdAt'] ?? raw['CreatedAt']);
  const u = raw['updatedAt'] ?? raw['UpdatedAt'];
  const updatedAt = u == null || u === '' ? null : str(u);
  return { id, name, description, isSystemRole, permissions, createdAt, updatedAt };
}

function parseRolesBody(body: unknown): RoleDto[] {
  if (Array.isArray(body)) {
    return body
      .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
      .map(normalizeRoleDto)
      .filter((r) => r.id || r.name);
  }
  if (body && typeof body === 'object') {
    const o = body as Record<string, unknown>;
    const success = o['success'] ?? o['Success'];
    const data = o['data'] ?? o['Data'];
    if (Array.isArray(data)) {
      return data
        .filter((x): x is Record<string, unknown> => x !== null && typeof x === 'object' && !Array.isArray(x))
        .map(normalizeRoleDto)
        .filter((r) => r.id || r.name);
    }
    if (success === true && (data === null || data === undefined)) {
      return [];
    }
  }
  return [];
}

@Injectable({ providedIn: 'root' })
export class RolesService {
  /** Toàn bộ global role (`AspNetRoles`), không phân trang — cùng nguồn với trang Roles. */
  private readonly api = `${environment.apiUrl}/api/roles`;

  constructor(private http: HttpClient) {}

  getRoles() {
    return this.http.get<unknown>(this.api).pipe(map(parseRolesBody));
  }

  getRole(id: string) {
    return this.http.get<ApiResponse<RoleDto>>(`${this.api}/${id}`);
  }

  createRole(data: { name: string; description: string; permissions?: string | null }) {
    return this.http.post<ApiResponse<RoleDto>>(this.api, data);
  }

  updateRole(id: string, data: { description?: string; permissions?: string | null }) {
    return this.http.put<ApiResponse<RoleDto>>(`${this.api}/${id}`, data);
  }

  deleteRole(id: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(`${this.api}/${id}`);
  }
}
