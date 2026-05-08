import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { SsoUserApplication } from '../../../core/models/api-response';
import { DropdownComponent, type DropdownOption } from '../../../shared/components/dropdown/dropdown.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-user-sso-access',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, EmptyStateComponent],
  template: `
    <div class="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(20rem,0.8fr)]">
      <section class="rounded-3xl border p-6 shadow-sm" [ngClass]="surfaceClasses()">
        <div class="mb-5 flex items-center justify-between gap-3">
          <div>
            <h3 class="text-lg font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">SSO application access</h3>
            <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
              Review every application currently assigned to this SSO account.
            </p>
          </div>
        </div>

        @if (applications().length) {
          <div class="space-y-3">
            @for (application of applications(); track application.id) {
              <div
                class="flex flex-col gap-3 rounded-2xl border px-4 py-4 transition duration-200 hover:-translate-y-px sm:flex-row sm:items-center sm:justify-between"
                [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50 hover:border-sky-200' : 'border-slate-800 bg-slate-950/70 hover:border-sky-500/30'"
              >
                <div>
                  <p class="font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">{{ application.name }}</p>
                  <p class="mt-1 text-xs uppercase tracking-[0.2em]" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
                    {{ application.role }}
                  </p>
                </div>
                <button
                  type="button"
                  class="inline-flex items-center justify-center rounded-xl border px-3 py-2 text-xs font-semibold transition duration-200 hover:-translate-y-px disabled:cursor-not-allowed disabled:opacity-60"
                  [ngClass]="theme() === 'light' ? 'border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100' : 'border-rose-500/30 bg-rose-500/10 text-rose-100 hover:bg-rose-500/20'"
                  [disabled]="revoking() === application.id"
                  (click)="revoke.emit(application.id)"
                >
                  {{ revoking() === application.id ? 'Revoking...' : 'Revoke access' }}
                </button>
              </div>
            }
          </div>
        } @else {
          <app-empty-state message="No SSO application access assigned yet." [theme]="theme()" />
        }
      </section>

      <section class="rounded-3xl border p-6 shadow-sm" [ngClass]="surfaceClasses()">
        <div class="mb-5">
          <h3 class="text-lg font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">Grant SSO application</h3>
          <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
            Choose an application and one of its roles.
          </p>
        </div>

        <div class="space-y-3">
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
            class="inline-flex w-full items-center justify-center gap-2 rounded-2xl bg-sky-500 px-4 py-3 text-sm font-semibold text-white transition duration-200 hover:-translate-y-px hover:bg-sky-600 disabled:cursor-not-allowed disabled:opacity-60"
            [disabled]="!selectedApplicationId() || !selectedRole() || granting()"
            (click)="grant.emit()"
          >
            <span class="material-symbols-outlined text-lg">link</span>
            {{ granting() ? 'Granting access...' : 'Grant access' }}
          </button>
        </div>

        @if (!applicationOptions().length && hasAvailableApplications()) {
          <p class="mt-4 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">
            Every application is already assigned to this account.
          </p>
        }
      </section>
    </div>
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

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/90'
      : 'border-slate-700 bg-slate-900/70';
  }
}
