import { CommonModule } from '@angular/common';
import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { SsoUserApplication } from '../../../core/models';
import { DropdownComponent, type DropdownOption } from '../../../shared/components/dropdown/dropdown.component';

@Component({
  selector: 'app-user-sso-access',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent],
  template: `
    <section class="rounded-[1.85rem] border p-5 shadow-[0_24px_80px_-52px_rgba(14,165,233,0.3)] sm:p-6" [ngClass]="surfaceClasses()">
      <div class="flex items-start justify-between gap-4">
        <button type="button" class="min-w-0 text-left" (click)="toggleExpanded()">
          <div class="flex flex-wrap items-center gap-3">
            <h3 class="text-lg font-semibold tracking-[-0.03em]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">SSO access</h3>
            <span class="inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white text-slate-600' : 'border-white/10 bg-white/[0.04] text-slate-300'">
              {{ applications().length }} linked
            </span>
          </div>
          <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
            Integrations stay visible, lightweight, and easy to revoke.
          </p>
        </button>

        <div class="flex items-center gap-2">
          <button
            type="button"
            class="inline-flex h-11 items-center justify-center rounded-full border px-4 text-sm font-semibold transition active:scale-[0.98]"
            [ngClass]="theme() === 'light'
              ? 'border-cyan-200 bg-cyan-50 text-cyan-700 hover:bg-cyan-100'
              : 'border-cyan-400/15 bg-cyan-400/12 text-cyan-100 hover:bg-cyan-400/18'"
            (click)="toggleComposer()"
          >
            <span class="material-symbols-outlined mr-1.5 text-[18px]">link</span>
            Link app
          </button>

          <button
            type="button"
            class="inline-flex h-11 w-11 items-center justify-center rounded-2xl border transition"
            [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white text-slate-600 hover:text-slate-950' : 'border-white/10 bg-white/[0.04] text-slate-300 hover:text-white'"
            (click)="toggleExpanded()"
            [attr.aria-label]="expanded() ? 'Collapse SSO access section' : 'Expand SSO access section'"
          >
            <span class="material-symbols-outlined text-[20px] transition-transform" [ngClass]="expanded() ? 'rotate-180' : ''">keyboard_arrow_down</span>
          </button>
        </div>
      </div>

      @if (expanded()) {
        <div class="mt-5 grid gap-4 xl:grid-cols-[minmax(0,1.25fr)_minmax(17rem,0.92fr)]">
          <div class="rounded-[1.55rem] border p-4 sm:p-5" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/85' : 'border-white/10 bg-white/[0.04]'">
            @if (applications().length) {
              <div class="space-y-3">
                @for (application of applications(); track application.id) {
                  <article
                    class="flex flex-col gap-3 rounded-[1.35rem] border px-4 py-4 transition hover:-translate-y-px sm:flex-row sm:items-center sm:justify-between"
                    [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50/70 hover:border-cyan-200' : 'border-white/10 bg-slate-950/55 hover:border-cyan-400/20'"
                  >
                    <div class="flex min-w-0 items-center gap-3">
                      <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-cyan-400 via-sky-500 to-indigo-600 text-sm font-semibold text-white shadow-[0_18px_38px_-24px_rgba(34,211,238,0.72)]">
                        {{ application.name.charAt(0).toUpperCase() }}
                      </div>
                      <div class="min-w-0">
                        <p class="truncate text-sm font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">{{ application.name }}</p>
                        <div class="mt-1 flex flex-wrap items-center gap-2">
                          <span class="inline-flex rounded-full border px-2.5 py-1 text-xs font-medium" [ngClass]="theme() === 'light' ? 'border-cyan-200 bg-cyan-50 text-cyan-700' : 'border-cyan-400/20 bg-cyan-400/12 text-cyan-100'">
                            {{ application.role }}
                          </span>
                          <span class="text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Linked workspace access</span>
                        </div>
                      </div>
                    </div>

                    <button
                      type="button"
                      class="inline-flex min-h-10 items-center justify-center rounded-full border px-3.5 py-2 text-xs font-semibold transition active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
                      [ngClass]="theme() === 'light' ? 'border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100' : 'border-rose-400/20 bg-rose-500/10 text-rose-100 hover:bg-rose-500/16'"
                      [disabled]="revoking() === application.id"
                      (click)="revoke.emit(application.id)"
                    >
                      {{ revoking() === application.id ? 'Removing...' : 'Remove' }}
                    </button>
                  </article>
                }
              </div>
            } @else {
              <div class="flex h-full flex-col items-center justify-center rounded-[1.4rem] border border-dashed px-5 py-10 text-center" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50/70' : 'border-white/10 bg-slate-950/40'">
                <div class="relative h-24 w-32">
                  <div class="absolute left-1/2 top-2 h-16 w-24 -translate-x-1/2 rounded-[1.4rem] border border-cyan-400/20 bg-cyan-400/10 shadow-[0_20px_45px_-30px_rgba(34,211,238,0.65)]"></div>
                  <div class="absolute left-2 top-10 h-12 w-12 rounded-[1rem] border border-white/10 bg-white/[0.05]"></div>
                  <div class="absolute right-2 top-10 h-12 w-12 rounded-[1rem] border border-white/10 bg-white/[0.03]"></div>
                  <div class="absolute left-1/2 top-0 h-10 w-10 -translate-x-1/2 rounded-2xl bg-gradient-to-br from-cyan-400 via-sky-500 to-indigo-600 shadow-[0_18px_36px_-20px_rgba(34,211,238,0.72)]"></div>
                </div>
                <p class="mt-5 text-sm font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-white'">No SSO apps yet</p>
                <p class="mt-1 max-w-sm text-sm" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
                  Link an application when this account needs external workspace access.
                </p>
              </div>
            }
          </div>

          @if (composerOpen()) {
            <div class="rounded-[1.55rem] border p-4 sm:p-5" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/92' : 'border-white/10 bg-slate-950/92'">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <h4 class="text-base font-semibold tracking-[-0.03em]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">Grant SSO application</h4>
                  <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
                    Pair one app with the right role and keep the drawer calm.
                  </p>
                </div>
                <button
                  type="button"
                  class="inline-flex h-9 w-9 items-center justify-center rounded-xl transition"
                  [ngClass]="theme() === 'light' ? 'text-slate-500 hover:bg-slate-100 hover:text-slate-900' : 'text-slate-400 hover:bg-white/[0.05] hover:text-white'"
                  (click)="closeComposer()"
                >
                  <span class="material-symbols-outlined text-[18px]">close</span>
                </button>
              </div>

              <div class="mt-4 space-y-3">
                <app-dropdown
                  [options]="applicationOptions()"
                  placeholder="Select an application"
                  [theme]="theme()"
                  [disabled]="!applicationOptions().length || granting()"
                  [ngModel]="selectedApplicationId()"
                  (ngModelChange)="selectedApplicationIdChange.emit($event)"
                />

                <app-dropdown
                  [options]="roleOptions()"
                  placeholder="Select an application role"
                  [theme]="theme()"
                  [disabled]="!selectedApplicationId() || !roleOptions().length || granting()"
                  [ngModel]="selectedRole()"
                  (ngModelChange)="selectedRoleChange.emit($event)"
                />

                <button
                  type="button"
                  class="inline-flex min-h-11 w-full items-center justify-center gap-2 rounded-full px-4 py-2.5 text-sm font-semibold transition active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
                  [ngClass]="theme() === 'light'
                    ? 'bg-slate-950 text-white hover:bg-slate-800'
                    : 'bg-cyan-400 text-slate-950 hover:bg-cyan-300'"
                  [disabled]="!selectedApplicationId() || !selectedRole() || granting()"
                  (click)="grant.emit()"
                >
                  <span class="material-symbols-outlined text-[18px]">link</span>
                  {{ granting() ? 'Linking...' : 'Link application' }}
                </button>
              </div>

              @if (!applicationOptions().length && hasAvailableApplications()) {
                <p class="mt-4 rounded-[1rem] border px-3 py-2 text-xs" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50 text-slate-500' : 'border-white/10 bg-white/[0.03] text-slate-400'">
                  Every available application is already linked to this account.
                </p>
              }
            </div>
          }
        </div>
      }
    </section>
  `
})
export class UserSsoAccessComponent {
  applications = input.required<SsoUserApplication[]>();
  applicationOptions = input.required<DropdownOption[]>();
  roleOptions = input.required<DropdownOption[]>();
  selectedApplicationId = input.required<string>();
  selectedRole = input.required<string>();
  granting = input<boolean>(false);
  revoking = input<string | null>(null);
  hasAvailableApplications = input<boolean>(false);
  theme = input<'light' | 'dark'>('dark');

  selectedApplicationIdChange = output<string>();
  selectedRoleChange = output<string>();
  grant = output<void>();
  revoke = output<string>();

  expanded = signal(true);
  composerOpen = signal(true);

  toggleExpanded() {
    this.expanded.set(!this.expanded());
  }

  toggleComposer() {
    this.composerOpen.set(!this.composerOpen());
  }

  closeComposer() {
    this.composerOpen.set(false);
  }

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/90'
      : 'border-white/10 bg-[linear-gradient(180deg,rgba(15,23,42,0.84),rgba(2,8,23,0.92))]';
  }
}
