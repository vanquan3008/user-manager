import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="motion-section rounded-3xl border px-6 py-8 text-center"
      [ngClass]="theme() === 'light'
        ? 'border-slate-200 bg-slate-50/60 text-slate-500'
        : 'border-slate-700 bg-slate-800/50 text-slate-400'"
    >
      <div class="mx-auto flex w-full max-w-3xl flex-col gap-5">
        <div class="grid gap-4 md:grid-cols-3">
          @for (item of skeletonCards; track item) {
            <div
              class="motion-shimmer h-28 rounded-3xl border"
              [ngClass]="theme() === 'light'
                ? 'border-slate-200 bg-white/80'
                : 'border-slate-700 bg-slate-900/45'"
            ></div>
          }
        </div>
        <div
          class="rounded-3xl border p-5 text-left"
          [ngClass]="theme() === 'light'
            ? 'border-slate-200 bg-white/80'
            : 'border-slate-700 bg-slate-900/35'"
        >
          <div class="space-y-3">
            <div class="motion-shimmer h-3.5 w-40 rounded-full" [ngClass]="theme() === 'light' ? 'bg-slate-200' : 'bg-slate-700'"></div>
            <div class="motion-shimmer h-3 rounded-full" [ngClass]="theme() === 'light' ? 'bg-slate-200/90' : 'bg-slate-700/90'"></div>
            <div class="motion-shimmer h-3 w-11/12 rounded-full" [ngClass]="theme() === 'light' ? 'bg-slate-200/80' : 'bg-slate-700/80'"></div>
            <div class="motion-shimmer h-3 w-8/12 rounded-full" [ngClass]="theme() === 'light' ? 'bg-slate-200/70' : 'bg-slate-700/70'"></div>
          </div>
        </div>
        <p class="text-sm font-medium">{{ message() }}</p>
      </div>
    </div>
  `
})
export class LoadingStateComponent {
  readonly skeletonCards = [0, 1, 2];
  message = input<string>('Loading...');
  theme = input<'light' | 'dark'>('dark');
}
