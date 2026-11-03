import { Pipe, PipeTransform } from '@angular/core';

/** Plan price in VND; -1 means unlimited/custom and renders as ∞. */
export function formatPlanPriceVnd(value: number | null | undefined): string {
  if (value === -1) return '∞';
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
