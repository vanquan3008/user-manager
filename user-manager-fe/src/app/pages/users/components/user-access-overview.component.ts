import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import type { IdentityUser } from '../../../core/models';

@Component({
  selector: 'app-user-access-overview',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="rounded-3xl border p-6 shadow-sm" [ngClass]="surfaceClasses()">
      <div class="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
        <div class="space-y-4">
          <div class="flex flex-wrap items-center gap-2">
            <span
              class="inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-[0.2em]"
              [ngClass]="theme() === 'light' ? 'bg-slate-900 text-white' : 'bg-white text-slate-900'"
            >
              {{ accountTypeLabel() }}
            </span>
            <span
              class="inline-flex items-center rounded-full px-3 py-1 text-xs font-medium"
              [ngClass]="user().emailConfirmed
                ? (theme() === 'light' ? 'bg-emerald-100 text-emerald-800' : 'bg-emerald-500/15 text-emerald-200')
                : (theme() === 'light' ? 'bg-amber-100 text-amber-800' : 'bg-amber-500/15 text-amber-200')"
            >
              {{ user().emailConfirmed ? 'Email verified' : 'Email pending' }}
            </span>
            <span
              class="inline-flex items-center rounded-full px-3 py-1 text-xs font-medium"
              [ngClass]="user().isBanned
                ? (theme() === 'light' ? 'bg-rose-100 text-rose-800' : 'bg-rose-500/15 text-rose-200')
                : (theme() === 'light' ? 'bg-sky-100 text-sky-800' : 'bg-sky-500/15 text-sky-200')"
            >
              {{ availabilityLabel() }}
            </span>
          </div>

          <div>
            <h2 class="text-2xl font-semibold tracking-tight" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
              {{ user().userName }}
            </h2>
            <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-300'">
              {{ user().email }}
            </p>
          </div>

          <div class="grid gap-3 sm:grid-cols-2">
            <div class="rounded-2xl border px-4 py-3" [ngClass]="statClasses()">
              <p class="text-xs uppercase tracking-[0.2em] text-slate-500">User ID</p>
              <p class="mt-2 break-all font-mono text-xs" [ngClass]="theme() === 'light' ? 'text-slate-700' : 'text-slate-300'">
                {{ user().id }}
              </p>
            </div>
            <div class="rounded-2xl border px-4 py-3" [ngClass]="statClasses()">
              <p class="text-xs uppercase tracking-[0.2em] text-slate-500">Created</p>
              <p class="mt-2 text-sm font-medium" [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-white'">
                {{ user().createdAt | date:'medium' }}
              </p>
            </div>
            <div class="rounded-2xl border px-4 py-3" [ngClass]="statClasses()">
              <p class="text-xs uppercase tracking-[0.2em] text-slate-500">Roles</p>
              <p class="mt-2 text-2xl font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                {{ assignedRoleCount() }}
              </p>
            </div>
            <div class="rounded-2xl border px-4 py-3" [ngClass]="statClasses()">
              <p class="text-xs uppercase tracking-[0.2em] text-slate-500">SSO Apps</p>
              <p class="mt-2 text-2xl font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">
                {{ ssoAppCount() }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>
  `
})
export class UserAccessOverviewComponent {
  user = input.required<IdentityUser>();
  assignedRoleCount = input.required<number>();
  ssoAppCount = input.required<number>();
  accountTypeLabel = input.required<string>();
  availabilityLabel = input.required<string>();
  theme = input<'light' | 'dark'>('dark');

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-sky-200 bg-gradient-to-br from-white via-sky-50 to-cyan-50'
      : 'border-sky-500/20 bg-gradient-to-br from-slate-900 via-slate-900 to-sky-950/70';
  }

  statClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white text-slate-900'
      : 'border-slate-800 bg-slate-950/70 text-slate-100';
  }
}
