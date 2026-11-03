import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  template: `
    <div class="space-y-3 p-5" role="status" aria-live="polite">
      @for (row of rowsArray(); track row) {
        <div class="flex items-center gap-3">
          <div class="skeleton h-8 w-8 rounded-full"></div>
          <div class="flex-1 space-y-2">
            <div class="skeleton h-3 w-1/3"></div>
            <div class="skeleton h-2.5 w-2/3"></div>
          </div>
        </div>
      }
      <span class="sr-only">{{ message() }}</span>
    </div>
  `
})
export class LoadingStateComponent {
  message = input<string>('Loading…');
  rows = input<number>(4);
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');
  rowsArray() { return Array.from({ length: this.rows() }, (_, i) => i); }
}
