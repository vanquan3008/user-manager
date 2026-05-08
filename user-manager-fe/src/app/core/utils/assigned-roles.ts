/** Role gán khi chưa chọn / rỗng — khớp `RoleNames.Guest` backend. */
export const DEFAULT_ASSIGNED_ROLE = 'Guest';

/** Đồng bộ với backend: AssignedRole là CSV (trim, không trùng). */
export function parseAssignedRolesCsv(csv: string | null | undefined): string[] {
  if (!csv?.trim()) return [];
  const parts = csv
    .split(',')
    .map(s => s.trim())
    .filter(s => s.length > 0);
  return [...new Set(parts)];
}

export function assignedRolesToCsv(roles: string[]): string {
  const normalized = [...new Set(roles.map(r => r.trim()).filter(r => r.length > 0))];
  return normalized.join(',');
}
