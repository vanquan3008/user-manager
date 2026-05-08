import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DropdownComponent, type DropdownOption } from '../../../shared/components/dropdown/dropdown.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-user-role-management',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, EmptyStateComponent],
  template: `
    <div class="grid gap-6 xl:grid-cols-[minmax(0,0.95fr)_minmax(0,1.4fr)]">
      <section class="rounded-3xl border p-5 shadow-sm" [ngClass]="surfaceClasses()">
        <div class="mb-4 flex items-center justify-between gap-3">
          <div>
            <h3 class="text-base font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">Add global role</h3>
            <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
              Assign a new platform role without leaving this screen.
            </p>
          </div>
        </div>

        <div class="space-y-3">
          <app-dropdown
            [options]="roleOptions()"
            [placeholder]="roleOptions().length ? 'Select a role' : 'No roles available'"
            [theme]="theme()"
            [disabled]="!roleOptions().length || addingRole()"
            [ngModel]="selectedRole()"
            (ngModelChange)="selectedRoleChange.emit($event)"
          />
          <button
            type="button"
            class="inline-flex w-full items-center justify-center gap-2 rounded-2xl bg-sky-500 px-4 py-3 text-sm font-semibold text-white transition duration-200 hover:-translate-y-px hover:bg-sky-600 disabled:cursor-not-allowed disabled:opacity-60"
            [disabled]="!selectedRole() || addingRole()"
            (click)="addRole.emit()"
          >
            <span class="material-symbols-outlined text-lg">add</span>
            {{ addingRole() ? 'Adding role...' : 'Add role' }}
          </button>
        </div>
      </section>

      <section class="rounded-3xl border p-6 shadow-sm" [ngClass]="surfaceClasses()">
        <div class="mb-5 flex items-center justify-between gap-3">
          <div>
            <h3 class="text-lg font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">Current roles</h3>
            <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
              Remove access that is no longer appropriate.
            </p>
          </div>
        </div>

        @if (roles().length) {
          <div class="grid gap-3 md:grid-cols-2">
            @for (role of roles(); track role) {
              <div
                class="group flex items-center justify-between gap-4 rounded-2xl border px-4 py-4 transition duration-200 hover:-translate-y-px"
                [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50 hover:border-sky-200' : 'border-slate-800 bg-slate-950/70 hover:border-sky-500/30'"
              >
                <div>
                  <p class="font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">{{ role }}</p>
                  <p class="mt-1 text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Global role assignment</p>
                </div>
                <button
                  type="button"
                  class="inline-flex items-center justify-center rounded-xl border px-3 py-2 text-xs font-semibold transition duration-200 hover:-translate-y-px disabled:cursor-not-allowed disabled:opacity-60"
                  [ngClass]="theme() === 'light' ? 'border-rose-200 bg-rose-50 text-rose-700 hover:bg-rose-100' : 'border-rose-500/30 bg-rose-500/10 text-rose-100 hover:bg-rose-500/20'"
                  [disabled]="removingRole() === role"
                  (click)="removeRole.emit(role)"
                >
                  {{ removingRole() === role ? 'Removing...' : 'Remove' }}
                </button>
              </div>
            }
          </div>
        } @else {
          <app-empty-state message="No global roles assigned yet." [theme]="theme()" />
        }
      </section>
    </div>
  `
})
export class UserRoleManagementComponent {
  roles = input.required<string[]>();
  roleOptions = input.required<DropdownOption[]>();
  selectedRole = input.required<string>();
  addingRole = input<boolean>(false);
  removingRole = input<string | null>(null);
  theme = input<'light' | 'dark'>('dark');

  selectedRoleChange = output<string>();
  addRole = output<void>();
  removeRole = output<string>();

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/90'
      : 'border-slate-700 bg-slate-900/70';
  }
}
