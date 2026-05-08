import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="app-page-header-card">
      <div class="app-page-header-card__inner">
      @if (backLink()) {
        <a
          [routerLink]="backLink()!"
          class="mb-2 inline-flex items-center gap-1.5 text-sm font-medium transition"
          [ngClass]="theme() === 'light' ? 'text-slate-600 hover:text-slate-900' : 'text-slate-400 hover:text-slate-100'"
        >
          <span class="material-symbols-outlined text-lg">arrow_back</span>
          {{ backLabel() }}
        </a>
      }
      <div class="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
        <div>
          @if (badge()) {
            <span
              class="app-header-toolbar-badge inline-flex items-center gap-1.5 rounded-md px-2.5 py-0.5 text-[10px] font-semibold uppercase tracking-[0.14em]"
            >
              {{ badge() }}
            </span>
          }
          <h1
            class="text-lg font-semibold tracking-tight sm:text-xl"
            [ngClass]="[
              theme() === 'light' ? 'text-slate-900' : 'text-slate-50',
              badge() ? 'mt-1.5' : 'mt-0'
            ]"
          >
            {{ title() }}
          </h1>
          @if (description()) {
            <p class="mt-1 text-sm leading-snug text-[var(--app-text-secondary)]">
              {{ description() }}
            </p>
          }
        </div>
        <div class="mt-2 flex items-center gap-2 sm:mt-0">
          <ng-content />
        </div>
      </div>
      </div>
    </div>
  `
})
export class PageHeaderComponent {
  title = input.required<string>();
  description = input<string>('');
  badge = input<string>('');
  backLink = input<string | null>(null);
  backLabel = input<string>('Back');
  theme = input<'light' | 'dark'>('dark');
}
