export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isSystemRole: boolean;
  permissions: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ApplicationRoleDto {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface PermissionDto {
  id: string;
  code: string;
  description: string;
  category: string;
  isSystemPermission: boolean;
  isActive: boolean;
  scope: string | null;
  createdAt: string;
}
export interface RoleWithPermissionsDto {
  id: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  permissions: PermissionDto[];
}
