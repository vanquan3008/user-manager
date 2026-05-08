import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="flex flex-col items-center justify-center rounded-xl border px-8 py-16 text-center"
      [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50/50' : 'border-slate-700 bg-slate-800/50'"
    >
      <span
        class="material-symbols-outlined mb-3 text-4xl opacity-50"
        [ngClass]="theme() === 'light' ? 'text-slate-400' : 'text-slate-500'"
      >
        {{ icon() }}
      </span>
      <p class="text-sm font-medium" [ngClass]="theme() === 'light' ? 'text-slate-700' : 'text-slate-300'">
        {{ message() }}
      </p>
      @if (action()) {
        <div class="mt-4 flex items-center gap-2">
          <ng-content select="[action]" />
        </div>
      }
    </div>
  `
})
export class EmptyStateComponent {
  message = input.required<string>();
  icon = input<string>('folder_off');
  action = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');
}
