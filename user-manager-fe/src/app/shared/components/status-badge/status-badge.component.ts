import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatusType = 'active' | 'banned' | 'inactive' | 'system';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold"
      [ngClass]="badgeClasses()"
    >
      <span class="h-1.5 w-1.5 rounded-full" [ngClass]="dotClasses()"></span>
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
      'bg-emerald-100 text-emerald-700 ring-1 ring-emerald-200': s === 'active' && isLight,
      'bg-emerald-500/15 text-emerald-200 ring-1 ring-emerald-500/20': s === 'active' && !isLight,
      'bg-rose-100 text-rose-700 ring-1 ring-rose-200': s === 'banned' && isLight,
      'bg-rose-500/15 text-rose-200 ring-1 ring-rose-500/20': s === 'banned' && !isLight,
      'bg-amber-100 text-amber-800 ring-1 ring-amber-200': s === 'system' && isLight,
      'bg-amber-500/15 text-amber-200 ring-1 ring-amber-500/20': s === 'system' && !isLight,
      'bg-slate-100 text-slate-600 ring-1 ring-slate-200': s === 'inactive' && isLight,
      'bg-slate-800/90 text-slate-300 ring-1 ring-slate-700': s === 'inactive' && !isLight
    };
  }

  dotClasses(): Record<string, boolean> {
    const s = this.status();
    return {
      'bg-emerald-500': s === 'active',
      'bg-rose-500': s === 'banned',
      'bg-amber-500': s === 'system',
      'bg-slate-400': s === 'inactive'
    };
  }
}
