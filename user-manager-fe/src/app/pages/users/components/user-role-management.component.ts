import { Component, computed, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateService } from '../../../core/services/translate.service';
import type { DropdownOption } from '../../../shared/components/dropdown/dropdown.component';

@Component({
  selector: 'app-user-role-management',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="card">
      <div class="card-header"><div><h2 class="card-title">{{ t().udRoles }}</h2><p class="card-desc">{{ t().udRolesDesc }}</p></div></div>
      <div class="card-body space-y-4">
        <ul class="flex flex-wrap gap-2">
          @for (role of sortedRoles(); track role) {
            <li class="inline-flex items-center gap-1 rounded-lg border border-line bg-surface-2 py-1 pl-2.5 pr-1 text-[13px] font-medium">
              <span class="material-symbols-outlined text-[16px] text-fg-3">{{ isPrivileged(role) ? 'admin_panel_settings' : 'person' }}</span>{{ role }}
              <button type="button" class="btn-icon-sm h-6 w-6" (click)="removeRole.emit(role)" [disabled]="removingRole() === role" [attr.aria-label]="t().udRemoveRole + ' ' + role">
                @if (removingRole() === role) { <span class="spinner h-3 w-3"></span> } @else { <span class="material-symbols-outlined text-[16px]">close</span> }
              </button>
            </li>
          } @empty {
            <li class="text-[13px] text-fg-3">{{ t().udNoRoles }}</li>
          }
        </ul>
        @if (roleOptions().length) {
          <div class="flex flex-wrap gap-2">
            <select class="input w-auto min-w-[200px]" [ngModel]="selectedRole()" (ngModelChange)="selectedRoleChange.emit($event)" [attr.aria-label]="t().udSelectRole">
              <option value="">{{ t().udSelectRole }}</option>
              @for (o of roleOptions(); track o.value) { <option [value]="o.value">{{ o.label }}</option> }
            </select>
            <button type="button" class="btn-secondary" (click)="addRole.emit()" [disabled]="!selectedRole() || addingRole()">
              @if (addingRole()) { <span class="spinner"></span> } @else { <span class="material-symbols-outlined">add</span> }{{ t().udAddRole }}
            </button>
          </div>
        }
      </div>
    </section>
  `
})
export class UserRoleManagementComponent {
  readonly t = inject(TranslateService).t;

  roles = input.required<string[]>();
  roleOptions = input.required<DropdownOption[]>();
  selectedRole = input.required<string>();
  addingRole = input<boolean>(false);
  removingRole = input<string | null>(null);
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');

  selectedRoleChange = output<string>();
  addRole = output<void>();
  removeRole = output<string>();

  private readonly order = ['superadmin', 'admin', 'user', 'guest'];
  sortedRoles = computed(() => [...this.roles()].sort((a, b) => this.rank(a) - this.rank(b) || a.localeCompare(b)));
  isPrivileged(role: string) { return this.rank(role) <= 1; }
  private rank(role: string) { const i = this.order.indexOf(role.toLowerCase()); return i < 0 ? 99 : i; }
}
