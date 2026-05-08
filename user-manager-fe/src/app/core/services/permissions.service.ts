import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse } from '../models/api-response';
import type { PermissionDto } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private readonly api = `${environment.apiUrl}/api/permissions`;
  private readonly rolesApi = `${environment.apiUrl}/api/roles`;

  constructor(private http: HttpClient) {}

  getPermissions() {
    return this.http.get<ApiResponse<PermissionDto[]>>(this.api).pipe(
      map(res => (res.success && res.data ? res.data : []))
    );
  }

  getPermission(id: string) {
    return this.http.get<ApiResponse<PermissionDto>>(`${this.api}/${id}`);
  }

  createPermission(data: { code: string; name?: string; resource?: string; action?: string; description?: string }) {
    return this.http.post<ApiResponse<PermissionDto>>(this.api, data);
  }

  getRolePermissions(roleId: string) {
    return this.http.get<ApiResponse<PermissionDto[]>>(`${this.rolesApi}/${roleId}/permissions`).pipe(
      map(res => (res.success && res.data ? res.data : []))
    );
  }

  addPermissionToRole(roleId: string, permissionId: string) {
    return this.http.post<ApiResponse<unknown>>(`${this.rolesApi}/${roleId}/permissions`, { permissionId });
  }

  addPermissionsToRoleBatch(roleId: string, permissionIds: string[]) {
    return this.http.post<ApiResponse<unknown>>(`${this.rolesApi}/${roleId}/permissions/batch`, { permissionIds });
  }

  removePermissionFromRole(roleId: string, permissionId: string) {
    return this.http.delete<ApiResponse<unknown>>(
      `${this.rolesApi}/${roleId}/permissions/${encodeURIComponent(permissionId)}`
    );
  }
}
