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
  name?: string;
  resource?: string;
  action?: string;
  description?: string;
  category?: string;
  isSystemPermission?: boolean;
  isActive?: boolean;
  scope?: string | null;
  createdAt?: string;
  updatedAt?: string | null;
}
