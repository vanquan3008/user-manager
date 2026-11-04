import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div>
      @if (backLink()) {
        <a [routerLink]="backLink()!" class="mb-3 inline-flex items-center gap-1 text-[13px] font-medium text-fg-2 transition hover:text-fg">
          <span class="material-symbols-outlined text-[18px]">arrow_back</span>{{ backLabel() }}
        </a>
      }
      <div class="page-header">
        <div class="min-w-0">
          @if (badge()) { <p class="eyebrow mb-1">{{ badge() }}</p> }
          <h1 class="page-title">{{ title() }}</h1>
          @if (description()) { <p class="page-subtitle">{{ description() }}</p> }
        </div>
        <div class="flex flex-wrap items-center gap-2"><ng-content /></div>
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
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
}
