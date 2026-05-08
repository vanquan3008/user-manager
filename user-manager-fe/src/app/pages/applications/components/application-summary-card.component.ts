import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import type { ApplicationListItem } from '../../../core/services/applications.service';

@Component({
  selector: 'app-application-summary-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <article
      class="motion-card group relative overflow-hidden rounded-lg border p-5 transition-all duration-300 hover:-translate-y-1 hover:shadow-2xl"
      [ngClass]="theme() === 'light'
        ? 'border-slate-200 bg-white shadow-sm hover:border-sky-300'
        : 'border-slate-700/70 bg-slate-800/85 shadow-slate-950/30 hover:border-sky-500/50 hover:bg-slate-800'"
    >
      <div
        class="pointer-events-none absolute inset-x-0 top-0 h-24 bg-gradient-to-br opacity-80 transition duration-300 group-hover:opacity-100"
        [ngClass]="theme() === 'light'
          ? 'from-sky-100 via-cyan-50 to-transparent'
          : 'from-sky-500/15 via-cyan-400/10 to-transparent'"
      ></div>

      <div class="relative flex h-full flex-col gap-5">
        <div class="flex items-start justify-between gap-3">
          <div class="flex min-w-0 items-center gap-3">
            <div
              class="flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-md border shadow-sm"
              [ngClass]="theme() === 'light'
                ? 'border-white/80 bg-white'
                : 'border-slate-700 bg-slate-900/70'"
            >
              <img
                [src]="app().logoUrl || defaultLogo()"
                [alt]="app().name + ' logo'"
                class="h-12 w-12 object-cover transition duration-300 group-hover:scale-105"
                (error)="handleLogoError($event)"
              />
            </div>
            <div class="min-w-0">
              <p
                class="truncate text-base font-semibold tracking-tight"
                [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-slate-50'"
              >
                {{ app().name }}
              </p>
              <p
                class="mt-1 truncate font-mono text-[11px]"
                [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'"
              >
                {{ app().id }}
              </p>
            </div>
          </div>
          <span
            class="rounded-md px-2.5 py-1 text-[11px] font-medium"
            [ngClass]="theme() === 'light'
              ? 'bg-slate-100 text-slate-600'
              : 'bg-slate-700/70 text-slate-300'"
          >
            {{ app().ownerId ? 'Owned' : 'Shared' }}
          </span>
        </div>

        <p
          class="min-h-10 text-sm leading-6"
          [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-300'"
        >
          {{ app().description || 'No description yet. Add context so admins know what this app is for.' }}
        </p>

        <div
          class="rounded-md border px-4 py-3"
          [ngClass]="theme() === 'light'
            ? 'border-slate-200 bg-slate-50/80'
            : 'border-slate-700/70 bg-slate-900/40'"
        >
          <p class="text-[11px] font-semibold uppercase tracking-[0.18em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
            Owner
          </p>
          <p
            class="mt-1 truncate text-xs"
            [ngClass]="theme() === 'light' ? 'text-slate-700' : 'text-slate-200'"
            [attr.title]="app().ownerDisplayName && app().ownerId ? app().ownerId! : null"
          >
            {{ app().ownerDisplayName || app().ownerId || 'Not assigned' }}
          </p>
        </div>

        <div class="mt-auto flex items-center justify-between gap-3">
          <a
            [routerLink]="['/applications', app().id]"
            class="inline-flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition"
            [ngClass]="theme() === 'light'
              ? 'bg-slate-900 text-white hover:bg-slate-700'
              : 'bg-sky-500 text-white hover:bg-sky-400'"
          >
            <span class="material-symbols-outlined text-[18px]">arrow_outward</span>
            Open
          </a>
          <button
            type="button"
            class="inline-flex items-center gap-2 rounded-md border px-3.5 py-2 text-sm font-medium transition disabled:pointer-events-none disabled:opacity-60"
            [disabled]="busy()"
            [ngClass]="theme() === 'light'
              ? 'border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100'
              : 'border-rose-500/40 bg-rose-500/10 text-rose-200 hover:bg-rose-500/20'"
            (click)="remove.emit()"
          >
            <span class="material-symbols-outlined text-[18px]">{{ busy() ? 'progress_activity' : 'delete' }}</span>
            {{ busy() ? 'Removing' : 'Delete' }}
          </button>
        </div>
      </div>
    </article>
  `
})
export class ApplicationSummaryCardComponent {
  app = input.required<ApplicationListItem>();
  theme = input<'light' | 'dark'>('dark');
  defaultLogo = input.required<string>();
  busy = input(false);

  remove = output<void>();

  handleLogoError(event: Event): void {
    const image = event.target as HTMLImageElement | null;
    if (image) {
      image.src = this.defaultLogo();
    }
  }
}
