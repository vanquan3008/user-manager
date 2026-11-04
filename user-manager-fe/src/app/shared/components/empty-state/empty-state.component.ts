import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center px-6 py-14 text-center">
      <div class="icon-tile h-11 w-11"><span class="material-symbols-outlined text-[22px]">{{ icon() }}</span></div>
      <p class="mt-4 text-sm font-medium text-fg">{{ message() }}</p>
      @if (description()) { <p class="mt-1 max-w-sm text-[13px] text-fg-2">{{ description() }}</p> }
      @if (action()) {
        <div class="mt-5 flex items-center gap-2"><ng-content select="[action]" /></div>
      }
    </div>
  `
})
export class EmptyStateComponent {
  message = input.required<string>();
  description = input<string>('');
  icon = input<string>('inbox');
  action = input<boolean>(false);
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
}
