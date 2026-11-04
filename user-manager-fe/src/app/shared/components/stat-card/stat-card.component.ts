import { Component, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';

/** KPI tile. With `link` set the whole tile navigates to the page that explains the number. */
@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [RouterLink, NgTemplateOutlet],
  template: `
    <ng-template #body>
      <div class="flex items-start justify-between gap-3">
        <p class="text-[13px] font-medium text-fg-2">{{ label() }}</p>
        @if (icon()) {
          <span class="flex h-8 w-8 items-center justify-center rounded-lg ring-1 ring-inset" [class]="toneClass()">
            <span class="material-symbols-outlined text-[18px]">{{ icon() }}</span>
          </span>
        }
      </div>
      <div>
        <p class="stat-value">{{ value() }}</p>
        <div class="mt-1.5 flex items-center justify-between gap-2">
          <p class="truncate text-xs text-fg-3">{{ sublabel() }}</p>
          @if (link()) {
            <span class="material-symbols-outlined text-[16px] text-fg-3 transition-transform group-hover:translate-x-0.5 group-hover:text-brand-text">arrow_forward</span>
          }
        </div>
      </div>
    </ng-template>

    @if (link()) {
      <a [routerLink]="link()" class="card group flex h-full flex-col justify-between gap-4 p-5 transition hover:border-line-strong hover:shadow-pop/5 focus-visible:ring-4 focus-visible:ring-brand/20">
        <ng-container [ngTemplateOutlet]="body" />
      </a>
    } @else {
      <div class="card flex h-full flex-col justify-between gap-4 p-5"><ng-container [ngTemplateOutlet]="body" /></div>
    }
  `
})
export class StatCardComponent {
  label = input.required<string>();
  value = input<string | number>('—');
  sublabel = input<string>('');
  icon = input<string>('');
  link = input<string>('');
  variant = input<'default' | 'success' | 'warning' | 'primary' | 'danger'>('default');
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');

  toneClass(): string {
    return {
      default: 'bg-surface-2 text-fg-2 ring-line',
      success: 'bg-ok-soft text-ok ring-ok/15',
      warning: 'bg-warn-soft text-warn ring-warn/15',
      primary: 'bg-brand-soft text-brand-text ring-brand/15',
      danger: 'bg-bad-soft text-bad ring-bad/15'
    }[this.variant()];
  }
}
