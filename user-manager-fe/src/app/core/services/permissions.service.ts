import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiResponse, SuccessResponse } from '../models/http.model';
import type { PermissionDto, RoleWithPermissionsDto } from '../models/role.model';

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private readonly api = `${environment.apiUrl}/api/permissions`;

  constructor(private http: HttpClient) {}

  getPermissions() {
    return this.http.get<ApiResponse<PermissionDto[]>>(this.api).pipe(
      map(res => (res.success && res.data ? res.data : []))
    );
  }

  getPermission(id: string) {
    return this.http.get<ApiResponse<PermissionDto>>(`${this.api}/${id}`);
  }

  createPermission(data: { code: string; description: string; category: string; scope?: string | null }) {
    return this.http.post<ApiResponse<PermissionDto>>(this.api, data);
  }

  getRolePermissions(roleId: string) {
    return this.http.get<ApiResponse<RoleWithPermissionsDto>>(`${this.api}/${roleId}/permissions`).pipe(
      map(res => (res.success && res.data ? res.data.permissions : []))
    );
  }

  addPermissionToRole(roleId: string, permissionId: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/${roleId}/permissions`, { permissionId });
  }

  addPermissionsToRoleBatch(roleId: string, permissionIds: string[]) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/${roleId}/permissions/batch`, { permissionIds });
  }

  removePermissionFromRole(roleId: string, permissionId: string) {
    return this.http.delete<ApiResponse<SuccessResponse>>(
      `${this.api}/${roleId}/permissions/${encodeURIComponent(permissionId)}`
    );
  }
}
