import { Pipe, PipeTransform } from '@angular/core';

/** Giá gói: số thường + " VND"; -1 = không giới hạn hiển thị "Tất cả". */
export function formatPlanPriceVnd(value: number | null | undefined): string {
  if (value === -1) return 'Tất cả';
  if (value == null || Number.isNaN(Number(value))) return '—';
  return `${Number(value).toLocaleString('vi-VN')} VND`;
}

@Pipe({
  name: 'planPriceVnd',
  standalone: true
})
export class PlanPriceVndPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    return formatPlanPriceVnd(value);
  }
}
