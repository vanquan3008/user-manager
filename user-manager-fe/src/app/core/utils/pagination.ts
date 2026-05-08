export function paginateSlice<T>(items: readonly T[], page: number, pageSize: number): T[] {
  if (pageSize <= 0 || items.length === 0) {
    return [];
  }
  const p = Math.max(1, page);
  const start = (p - 1) * pageSize;
  return items.slice(start, start + pageSize);
}

export function totalListPages(totalItems: number, pageSize: number): number {
  if (pageSize <= 0 || totalItems <= 0) {
    return 1;
  }
  return Math.max(1, Math.ceil(totalItems / pageSize));
}

export function clampPage(page: number, totalItems: number, pageSize: number): number {
  const max = totalListPages(totalItems, pageSize);
  if (page < 1) {
    return 1;
  }
  if (page > max) {
    return max;
  }
  return page;
}
