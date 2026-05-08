import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="motion-card group relative flex flex-col overflow-hidden rounded-xl border transition-all duration-300 hover:-translate-y-1 hover:shadow-xl"
      [ngClass]="cardClasses()"
    >
      <div
        class="pointer-events-none absolute inset-x-0 top-0 h-20 bg-gradient-to-br opacity-80 transition duration-300 group-hover:opacity-100"
        [ngClass]="theme() === 'light'
          ? 'from-white/60 via-sky-50/40 to-transparent'
          : 'from-white/5 via-sky-400/5 to-transparent'"
      ></div>
      <div class="relative flex flex-col gap-3 p-5">
        <div class="flex items-center justify-between">
          @if (icon()) {
            <span
              class="material-symbols-outlined rounded-lg p-2 text-2xl"
              [ngClass]="theme() === 'light' ? iconLightClass() : 'text-slate-400'"
            >
              {{ icon() }}
            </span>
          }
          <span class="max-w-[70%] truncate text-2xl font-bold" [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-slate-50'">
            {{ value() }}
          </span>
        </div>
        <p class="text-[11px] font-semibold uppercase tracking-[0.18em]" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-300'">
          {{ label() }}
        </p>
        @if (sublabel()) {
          <p class="text-xs font-medium leading-5" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
            {{ sublabel() }}
          </p>
        }
      </div>
    </div>
  `
})
export class StatCardComponent {
  label = input.required<string>();
  value = input<string | number>('—');
  sublabel = input<string>('');
  icon = input<string>('');
  variant = input<'default' | 'success' | 'warning' | 'primary'>('default');
  theme = input<'light' | 'dark'>('dark');

  cardClasses(): Record<string, boolean> {
    const t = this.theme();
    const v = this.variant();
    const isLight = t === 'light';
    return {
      'border-slate-200 bg-white hover:border-slate-300': isLight && v === 'default',
      'border-slate-700/50 bg-slate-800/80 hover:border-slate-600': !isLight && v === 'default',
      'border-emerald-200 bg-emerald-50/80 hover:border-emerald-300': isLight && v === 'success',
      'border-emerald-500/40 bg-emerald-900/20 hover:border-emerald-500/60': !isLight && v === 'success',
      'border-rose-200 bg-rose-50/80 hover:border-rose-300': isLight && v === 'warning',
      'border-rose-500/40 bg-rose-900/20 hover:border-rose-500/60': !isLight && v === 'warning',
      'border-sky-200 bg-sky-50/80 hover:border-sky-300': isLight && v === 'primary',
      'border-sky-500/40 bg-sky-900/20 hover:border-sky-500/60': !isLight && v === 'primary'
    };
  }

  iconLightClass(): string {
    const v = this.variant();
    const map: Record<string, string> = {
      default: 'bg-slate-100 text-slate-600',
      success: 'bg-emerald-100 text-emerald-700',
      warning: 'bg-rose-100 text-rose-700',
      primary: 'bg-sky-100 text-sky-700'
    };
    return map[v] ?? 'text-slate-600';
  }
}
