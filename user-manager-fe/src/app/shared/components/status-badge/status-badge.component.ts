import { Component, input } from '@angular/core';

export type StatusType = 'active' | 'banned' | 'inactive' | 'system';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span [class]="badgeClass()"><span class="dot"></span>{{ displayLabel }}</span>`
})
export class StatusBadgeComponent {
  status = input.required<StatusType>();
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
  customLabel = input<string | null>(null);

  get displayLabel(): string {
    return this.customLabel() ?? ({ active: 'Active', banned: 'Banned', inactive: 'Inactive', system: 'System' } as const)[this.status()];
  }

  badgeClass(): string {
    return ({ active: 'badge-ok', banned: 'badge-bad', inactive: 'badge-neutral', system: 'badge-warn' } as const)[this.status()];
  }
}
