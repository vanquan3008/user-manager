import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { SsoUserApplication } from '../../../core/models';
import { TranslateService } from '../../../core/services/translate.service';
import type { DropdownOption } from '../../../shared/components/dropdown/dropdown.component';

@Component({
  selector: 'app-user-sso-access',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="card">
      <div class="card-header"><div><h2 class="card-title">{{ t().udApps }}</h2><p class="card-desc">{{ t().udAppsDesc }}</p></div></div>
      <ul class="divide-y divide-line">
        @for (app of applications(); track app.id) {
          <li class="flex items-center gap-3 px-5 py-3">
            <span class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-brand-soft text-xs font-semibold text-brand-text">{{ app.name.charAt(0).toUpperCase() }}</span>
            <div class="min-w-0 flex-1"><p class="truncate text-sm font-medium">{{ app.name }}</p></div>
            <span class="badge-neutral">{{ app.role }}</span>
            <button type="button" class="btn-danger-ghost btn-sm" (click)="revoke.emit(app.id)" [disabled]="revoking() === app.id">
              @if (revoking() === app.id) { <span class="spinner h-3 w-3"></span> } {{ t().udRevoke }}
            </button>
          </li>
        } @empty {
          <li class="px-5 py-6 text-center text-[13px] text-fg-3">{{ t().udNoApps }}</li>
        }
      </ul>
      <div class="card-footer flex-wrap justify-start">
        @if (!hasAvailableApplications()) {
          <p class="text-[13px] text-fg-3">{{ t().udNoAppsAvailable }}</p>
        } @else {
          <select class="input w-auto min-w-[200px]" [ngModel]="selectedApplicationId()" (ngModelChange)="selectedApplicationIdChange.emit($event)" [attr.aria-label]="t().udSelectApp">
            <option value="">{{ t().udSelectApp }}</option>
            @for (o of applicationOptions(); track o.value) { <option [value]="o.value">{{ o.label }}</option> }
          </select>
          <select class="input w-auto min-w-[160px]" [ngModel]="selectedRole()" (ngModelChange)="selectedRoleChange.emit($event)" [disabled]="!selectedApplicationId() || !roleOptions().length" [attr.aria-label]="t().udSelectAppRole">
            <option value="">{{ t().udSelectAppRole }}</option>
            @for (o of roleOptions(); track o.value) { <option [value]="o.value">{{ o.label }}</option> }
          </select>
          <button type="button" class="btn-primary" (click)="grant.emit()" [disabled]="!selectedApplicationId() || !selectedRole() || granting()">
            @if (granting()) { <span class="spinner"></span> } {{ t().udGrant }}
          </button>
        }
      </div>
    </section>
  `
})
export class UserSsoAccessComponent {
  readonly t = inject(TranslateService).t;

  applications = input.required<SsoUserApplication[]>();
  applicationOptions = input.required<DropdownOption[]>();
  roleOptions = input.required<DropdownOption[]>();
  selectedApplicationId = input.required<string>();
  selectedRole = input.required<string>();
  granting = input<boolean>(false);
  revoking = input<string | null>(null);
  hasAvailableApplications = input<boolean>(false);
  /** @deprecated kept for template compatibility */
  theme = input<'light' | 'dark'>('dark');

  selectedApplicationIdChange = output<string>();
  selectedRoleChange = output<string>();
  grant = output<void>();
  revoke = output<string>();
}
