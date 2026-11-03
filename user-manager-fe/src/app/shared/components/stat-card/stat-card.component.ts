import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  template: `
    <div class="card flex h-full flex-col justify-between gap-4 p-5">
      <div class="flex items-start justify-between gap-3">
        <p class="text-[13px] font-medium text-fg-2">{{ label() }}</p>
        @if (icon()) {
          <span class="flex h-8 w-8 items-center justify-center rounded-lg" [class]="toneClass()">
            <span class="material-symbols-outlined text-[18px]">{{ icon() }}</span>
          </span>
        }
      </div>
      <div>
        <p class="stat-value">{{ value() }}</p>
        @if (sublabel()) { <p class="mt-1.5 text-xs text-fg-3">{{ sublabel() }}</p> }
      </div>
    </div>
  `
})
export class StatCardComponent {
  label = input.required<string>();
  value = input<string | number>('—');
  sublabel = input<string>('');
  icon = input<string>('');
  variant = input<'default' | 'success' | 'warning' | 'primary' | 'danger'>('default');
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');

  toneClass(): string {
    return {
      default: 'bg-surface-2 text-fg-2',
      success: 'bg-ok-soft text-ok',
      warning: 'bg-warn-soft text-warn',
      primary: 'bg-brand-soft text-brand-text',
      danger: 'bg-bad-soft text-bad'
    }[this.variant()];
  }
}
