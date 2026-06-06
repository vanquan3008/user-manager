import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="motion-card group relative overflow-hidden rounded-[1.6rem] border transition-all duration-300 hover:-translate-y-1.5 hover:scale-105"
      [ngClass]="cardClasses()"
    >
      <div
        class="pointer-events-none absolute inset-0 opacity-90 transition duration-300 group-hover:opacity-100"
        [ngClass]="theme() === 'light'
          ? 'bg-gradient-to-b from-white/40 to-transparent'
          : 'bg-gradient-to-b from-white/5 to-transparent'"
      ></div>
      <div class="relative flex flex-col gap-5 p-5">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0">
            <p class="text-[11px] font-extrabold uppercase tracking-[0.22em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
              {{ label() }}
            </p>
            <div class="mt-3 flex items-end gap-3">
              <span class="truncate text-3xl font-bold tracking-tight sm:text-[2.1rem]" style="font-family: 'Space Grotesk', 'Manrope', sans-serif;" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                {{ value() }}
              </span>
              @if (sublabel()) {
                <span class="max-w-[10.5rem] text-xs font-medium leading-5" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
                  {{ sublabel() }}
                </span>
              }
            </div>
          </div>

          @if (icon()) {
            <span
              class="material-symbols-outlined rounded-[1rem] p-3 text-[26px] shadow-sm ring-1 transition-all duration-300 group-hover:scale-110 group-hover:-rotate-3 group-hover:shadow-lg"
              [ngClass]="iconChipClasses()"
            >
              {{ icon() }}
            </span>
          }
        </div>

        <div class="flex items-center justify-between gap-3">
          <div class="h-2 flex-1 overflow-hidden rounded-full" [ngClass]="theme() === 'light' ? 'bg-white/70' : 'bg-slate-900/70'">
            <div class="h-full rounded-full" [ngClass]="accentBarClasses()"></div>
          </div>
          <span class="text-[11px] font-semibold uppercase tracking-[0.18em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
            Live
          </span>
        </div>
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
      'border-white/75 bg-white/88 shadow-xl hover:border-slate-300/80 hover:shadow-2xl': isLight && v === 'default',
      'border-slate-700/40 bg-slate-900/72 shadow-xl hover:border-slate-500/60 hover:shadow-2xl': !isLight && v === 'default',
      'border-emerald-200/75 bg-emerald-50/90 shadow-xl hover:border-emerald-300/90 hover:shadow-2xl': isLight && v === 'success',
      'border-emerald-400/28 bg-emerald-950/35 shadow-xl hover:border-emerald-300/50 hover:shadow-2xl': !isLight && v === 'success',
      'border-amber-200/78 bg-amber-50/90 shadow-xl hover:border-amber-300/90 hover:shadow-2xl': isLight && v === 'warning',
      'border-amber-400/28 bg-amber-950/35 shadow-xl hover:border-amber-300/50 hover:shadow-2xl': !isLight && v === 'warning',
      'border-sky-200/78 bg-sky-50/90 shadow-xl hover:border-sky-300/90 hover:shadow-2xl': isLight && v === 'primary',
      'border-sky-400/28 bg-sky-950/35 shadow-xl hover:border-sky-300/52 hover:shadow-2xl': !isLight && v === 'primary'
    };
  }

  iconChipClasses(): string {
    const v = this.variant();
    const isLight = this.theme() === 'light';
    const map: Record<string, { light: string; dark: string }> = {
      default: {
        light: 'bg-slate-100 text-slate-700 ring-slate-200',
        dark: 'bg-slate-800/90 text-slate-200 ring-slate-700/70'
      },
      success: {
        light: 'bg-emerald-100 text-emerald-700 ring-emerald-200',
        dark: 'bg-emerald-400/15 text-emerald-200 ring-emerald-400/20'
      },
      warning: {
        light: 'bg-amber-100 text-amber-700 ring-amber-200',
        dark: 'bg-amber-400/15 text-amber-200 ring-amber-400/20'
      },
      primary: {
        light: 'bg-sky-100 text-sky-700 ring-sky-200',
        dark: 'bg-sky-400/15 text-sky-200 ring-sky-400/20'
      }
    };
    return isLight ? map[v]?.light ?? map['default'].light : map[v]?.dark ?? map['default'].dark;
  }

  accentBarClasses(): string {
    const v = this.variant();
    const map: Record<string, string> = {
      default: 'w-[38%] bg-slate-400',
      success: 'w-[64%] bg-emerald-500',
      warning: 'w-[48%] bg-amber-500',
      primary: 'w-[72%] bg-sky-500'
    };
    return map[v] ?? map['default'];
  }
}
