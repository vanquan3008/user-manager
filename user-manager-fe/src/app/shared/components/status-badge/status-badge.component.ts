import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatusType = 'active' | 'banned' | 'inactive' | 'system';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
      [ngClass]="badgeClasses()"
    >
      {{ displayLabel }}
    </span>
  `
})
export class StatusBadgeComponent {
  status = input.required<StatusType>();
  theme = input<'light' | 'dark'>('dark');
  customLabel = input<string | null>(null);

  get displayLabel(): string {
    const custom = this.customLabel();
    if (custom) return custom;
    const labels: Record<StatusType, string> = {
      active: 'Active',
      banned: 'Banned',
      inactive: 'Inactive',
      system: 'System'
    };
    return labels[this.status()] ?? '';
  }

  badgeClasses(): Record<string, boolean> {
    const s = this.status();
    const t = this.theme();
    const isLight = t === 'light';
    return {
      'bg-emerald-100 text-emerald-700': s === 'active' && isLight,
      'bg-emerald-500/20 text-emerald-300': s === 'active' && !isLight,
      'bg-rose-100 text-rose-700': s === 'banned' && isLight,
      'bg-rose-500/20 text-rose-300': s === 'banned' && !isLight,
      'bg-amber-100 text-amber-800': s === 'system' && isLight,
      'bg-amber-500/20 text-amber-200': s === 'system' && !isLight,
      'bg-slate-100 text-slate-600': s === 'inactive' && isLight,
      'bg-slate-600/50 text-slate-400': s === 'inactive' && !isLight
    };
  }
}
