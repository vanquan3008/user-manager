/**
 * Giới hạn gói (form + payload API): trống / null / không phải số → -1 (không giới hạn).
 * Dùng khi gửi request và khi map dữ liệu API vào form.
 */
export function parsePlanLimitField(raw: unknown): number {
  if (raw === null || raw === undefined || raw === '') return -1;
  const n = Number(raw);
  if (Number.isNaN(n)) return -1;
  if (n < -1) return -1;
  return Math.trunc(n);
}
