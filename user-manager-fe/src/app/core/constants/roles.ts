import type { SsoApplication } from '../models';

/**
 * Role names that are allowed to access the User Admin UI (dashboard, users, roles, subscriptions, etc.).
 * Backend auth (login / magic-link) must return user.roles containing at least one of these strings.
 * Keep in sync with your backend (e.g. Identity role names).
 */
export const ALLOWED_ADMIN_ROLES = ['Admin', 'SuperAdmin'] as const;

export type AllowedAdminRole = (typeof ALLOWED_ADMIN_ROLES)[number];

export function hasAdminRole(roles: string[] | undefined | null): boolean {
  if (!roles?.length) return false;
  return roles.some((r) => ALLOWED_ADMIN_ROLES.includes(r as AllowedAdminRole));
}

export function isSuperAdmin(roles: string[] | undefined | null): boolean {
  return roles?.includes('SuperAdmin') ?? false;
}

/** Ban/unban user: Admin/SuperAdmin toàn cục, hoặc SSO có quản lý user trên app (canManageUsers / role Admin). */
export function canBanUsers(
  roles: string[] | undefined | null,
  applications: SsoApplication[] | undefined | null
): boolean {
  if (hasAdminRole(roles)) return true;
  if (!applications?.length) return false;
  return applications.some(
    (a) => a.canManageUsers === true || /^admin$/i.test((a.role ?? '').trim())
  );
}
