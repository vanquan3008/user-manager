import { CommonModule } from '@angular/common';
import { Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DropdownComponent, type DropdownOption } from '../../../shared/components/dropdown/dropdown.component';

@Component({
  selector: 'app-user-role-management',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent],
  template: `
    <section class="rounded-[1.85rem] border p-5 shadow-[0_24px_80px_-52px_rgba(14,165,233,0.34)] sm:p-6" [ngClass]="surfaceClasses()">
      <div class="flex items-start justify-between gap-4">
        <button type="button" class="min-w-0 text-left" (click)="toggleExpanded()">
          <div class="flex flex-wrap items-center gap-3">
            <h3 class="text-lg font-semibold tracking-[-0.03em]" [ngClass]="theme() === 'light' ? 'text-slate-950' : 'text-white'">Roles</h3>
            <span class="inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white text-slate-600' : 'border-white/10 bg-white/[0.04] text-slate-300'">
              {{ roles().length }} assigned
            </span>
          </div>
          <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-600' : 'text-slate-400'">
            Fast edits, cleaner role pills, and hierarchy at a glance.
          </p>
        </button>

        <div class="flex items-center gap-2">
          <div class="hidden items-center gap-2 rounded-full border px-3 py-2 text-xs sm:flex" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/85 text-slate-600' : 'border-white/10 bg-white/[0.04] text-slate-300'">
            <span class="material-symbols-outlined text-sm">account_tree</span>
            @for (role of hierarchyPreview(); track role) {
              <span>{{ role }}</span>
              @if (!$last) {
                <span class="opacity-40">></span>
              }
            }
          </div>

          <button
            type="button"
            class="inline-flex h-11 items-center justify-center rounded-full border px-4 text-sm font-semibold transition active:scale-[0.98]"
            [ngClass]="theme() === 'light'
              ? 'border-cyan-200 bg-cyan-50 text-cyan-700 hover:bg-cyan-100'
              : 'border-cyan-400/15 bg-cyan-400/12 text-cyan-100 hover:bg-cyan-400/18'"
            (click)="toggleComposer()"
          >
            <span class="material-symbols-outlined mr-1.5 text-[18px]">add</span>
            Add role
          </button>

          <button
            type="button"
            class="inline-flex h-11 w-11 items-center justify-center rounded-2xl border transition"
            [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white text-slate-600 hover:text-slate-950' : 'border-white/10 bg-white/[0.04] text-slate-300 hover:text-white'"
            (click)="toggleExpanded()"
            [attr.aria-label]="expanded() ? 'Collapse roles section' : 'Expand roles section'"
          >
            <span class="material-symbols-outlined text-[20px] transition-transform" [ngClass]="expanded() ? 'rotate-180' : ''">keyboard_arrow_down</span>
          </button>
        </div>
      </div>

      @if (composerOpen()) {
        <div class="relative mt-4">
          <div class="rounded-[1.5rem] border p-4 shadow-[0_26px_60px_-42px_rgba(14,165,233,0.42)] sm:max-w-[24rem]" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white/92' : 'border-white/10 bg-slate-950/92'">
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="text-sm font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-900' : 'text-white'">Add a global role</p>
                <p class="mt-1 text-xs" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Pick the next role without crowding the main panel.</p>
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
                [options]="roleOptions()"
                [placeholder]="roleOptions().length ? 'Select a role' : 'No roles available'"
                [theme]="theme()"
                [disabled]="!roleOptions().length || addingRole()"
                [ngModel]="selectedRole()"
                (ngModelChange)="selectedRoleChange.emit($event)"
              />

              <div class="flex items-center gap-2">
                <button
                  type="button"
                  class="inline-flex min-h-11 flex-1 items-center justify-center gap-2 rounded-full px-4 py-2.5 text-sm font-semibold transition active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60"
                  [ngClass]="theme() === 'light'
                    ? 'bg-slate-950 text-white hover:bg-slate-800'
                    : 'bg-cyan-400 text-slate-950 hover:bg-cyan-300'"
                  [disabled]="!selectedRole() || addingRole()"
                  (click)="submitRole()"
                >
                  <span class="material-symbols-outlined text-[18px]">spark</span>
                  {{ addingRole() ? 'Adding...' : 'Assign role' }}
                </button>

                <button
                  type="button"
                  class="inline-flex min-h-11 items-center justify-center rounded-full border px-4 py-2.5 text-sm font-medium transition"
                  [ngClass]="theme() === 'light' ? 'border-slate-200 bg-white text-slate-600 hover:text-slate-900' : 'border-white/10 bg-white/[0.04] text-slate-300 hover:text-white'"
                  (click)="closeComposer()"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      }

      @if (expanded()) {
        <div class="mt-5">
          @if (sortedRoles().length) {
            <div class="flex flex-wrap gap-3">
              @for (role of sortedRoles(); track role) {
                <div class="group flex items-center gap-2 rounded-full border px-3 py-2.5 transition hover:-translate-y-px" [ngClass]="chipClasses(role)">
                  <span class="inline-flex h-7 w-7 items-center justify-center rounded-full text-[11px] font-semibold" [ngClass]="chipDotClasses(role)">
                    {{ tierLabel(role) }}
                  </span>
                  <span class="text-sm font-semibold">{{ role }}</span>
                  <button
                    type="button"
                    class="inline-flex h-7 w-7 items-center justify-center rounded-full transition disabled:cursor-not-allowed disabled:opacity-60"
                    [ngClass]="theme() === 'light' ? 'text-slate-400 hover:bg-slate-100 hover:text-rose-600' : 'text-slate-400 hover:bg-white/[0.08] hover:text-rose-200'"
                    [disabled]="removingRole() === role"
                    [attr.title]="removingRole() === role ? 'Removing role' : 'Remove role'"
                    (click)="removeRole.emit(role)"
                  >
                    <span class="material-symbols-outlined text-[18px]">{{ removingRole() === role ? 'progress_activity' : 'close' }}</span>
                  </button>
                </div>
              }
            </div>
          } @else {
            <div class="rounded-[1.5rem] border border-dashed px-5 py-8 text-center" [ngClass]="theme() === 'light' ? 'border-slate-200 bg-slate-50/70' : 'border-white/10 bg-white/[0.03]'">
              <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-cyan-400/12 text-cyan-100 ring-1 ring-cyan-400/15">
                <span class="material-symbols-outlined text-[22px]">shield_lock</span>
              </div>
              <p class="mt-4 text-sm font-semibold" [ngClass]="theme() === 'light' ? 'text-slate-800' : 'text-slate-100'">No global roles yet</p>
              <p class="mt-1 text-sm" [ngClass]="theme() === 'light' ? 'text-slate-500' : 'text-slate-400'">Use the add-role popover to grant the first permission set.</p>
            </div>
          }
        </div>
      }
    </section>
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

  expanded = signal(true);
  composerOpen = signal(false);

  sortedRoles = computed(() => [...this.roles()].sort((left, right) => this.rolePriority(left) - this.rolePriority(right) || left.localeCompare(right)));
  hierarchyPreview = computed(() => {
    const names = new Set<string>();
    for (const role of [...this.roles(), ...this.roleOptions().map(option => option.label)]) {
      const normalized = role.trim();
      if (normalized) {
        names.add(normalized);
      }
    }

    return [...names]
      .sort((left, right) => this.rolePriority(left) - this.rolePriority(right) || left.localeCompare(right))
      .slice(0, 4);
  });

  toggleComposer() {
    this.composerOpen.set(!this.composerOpen());
  }

  toggleExpanded() {
    this.expanded.set(!this.expanded());
  }

  closeComposer() {
    this.composerOpen.set(false);
  }

  submitRole() {
    if (!this.selectedRole() || this.addingRole()) {
      return;
    }

    this.addRole.emit();
    this.composerOpen.set(false);
  }

  surfaceClasses() {
    return this.theme() === 'light'
      ? 'border-slate-200 bg-white/90'
      : 'border-white/10 bg-[linear-gradient(180deg,rgba(15,23,42,0.84),rgba(2,8,23,0.92))]';
  }

  chipClasses(role: string) {
    const tier = this.roleTier(role);
    if (this.theme() === 'light') {
      if (tier === 'high') return 'border-cyan-200 bg-cyan-50 text-cyan-800';
      if (tier === 'mid') return 'border-slate-200 bg-white text-slate-800';
      return 'border-slate-200 bg-slate-50 text-slate-700';
    }

    if (tier === 'high') return 'border-cyan-400/20 bg-cyan-400/12 text-cyan-100';
    if (tier === 'mid') return 'border-white/10 bg-white/[0.05] text-slate-100';
    return 'border-white/10 bg-white/[0.03] text-slate-300';
  }

  chipDotClasses(role: string) {
    const tier = this.roleTier(role);
    if (this.theme() === 'light') {
      if (tier === 'high') return 'bg-cyan-100 text-cyan-700';
      if (tier === 'mid') return 'bg-slate-100 text-slate-700';
      return 'bg-slate-200 text-slate-600';
    }

    if (tier === 'high') return 'bg-cyan-300/18 text-cyan-100';
    if (tier === 'mid') return 'bg-white/[0.08] text-slate-100';
    return 'bg-white/[0.06] text-slate-300';
  }

  tierLabel(role: string) {
    const tier = this.roleTier(role);
    if (tier === 'high') return 'A';
    if (tier === 'mid') return 'B';
    return 'C';
  }

  private roleTier(role: string) {
    const priority = this.rolePriority(role);
    if (priority <= 2) return 'high';
    if (priority <= 5) return 'mid';
    return 'low';
  }

  private rolePriority(roleName: string) {
    const normalized = roleName.trim().toLowerCase();
    if (normalized.includes('superadmin')) return 0;
    if (normalized.includes('admin')) return 1;
    if (normalized.includes('owner')) return 2;
    if (normalized.includes('manager')) return 3;
    if (normalized.includes('editor')) return 4;
    if (normalized.includes('analyst')) return 5;
    if (normalized.includes('viewer')) return 6;
    if (normalized.includes('user')) return 7;
    return 8;
  }
}
