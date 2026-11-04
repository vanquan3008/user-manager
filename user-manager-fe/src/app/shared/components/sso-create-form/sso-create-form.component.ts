import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormFieldComponent } from '../form-field/form-field.component';

export interface SsoFormModel {
  email: string;
  password: string;
  username: string;
  firstName: string;
  lastName: string;
  /** ISO `YYYY-MM-DD`, required by API (domain: age 13–120). */
  dateOfBirth: string;
}

@Component({
  selector: 'app-sso-create-form',
  standalone: true,
  imports: [CommonModule, FormsModule, FormFieldComponent],
  template: `
    @if (info()) {
      <p class="alert-info mb-4"><span class="material-symbols-outlined">info</span><span>{{ info() }}</span></p>
    }
    <div class="flex flex-col gap-4">
      <app-form-field
        label="Email"
        [model]="form().email"
        (modelChange)="form().email = $event"
        placeholder="sso@example.com"
        type="email"
        [required]="true"
        [theme]="theme()"
      />
      <app-form-field
        label="Password"
        [model]="form().password"
        (modelChange)="form().password = $event"
        placeholder="••••••••"
        type="password"
        [required]="true"
        [theme]="theme()"
      />
      <app-form-field
        label="Username"
        [model]="form().username"
        (modelChange)="form().username = $event"
        placeholder="sso_user"
        [required]="true"
        [theme]="theme()"
      />
      <div class="grid grid-cols-2 gap-4">
        <app-form-field
          label="First name"
          [model]="form().firstName"
          (modelChange)="form().firstName = $event"
          placeholder="First"
          [theme]="theme()"
        />
        <app-form-field
          label="Last name"
          [model]="form().lastName"
          (modelChange)="form().lastName = $event"
          placeholder="Last"
          [theme]="theme()"
        />
      </div>
      <app-form-field
        label="Date of birth"
        [model]="form().dateOfBirth"
        (modelChange)="form().dateOfBirth = $event"
        type="date"
        [min]="minDobIso()"
        [max]="maxDobIso()"
        [required]="true"
        [theme]="theme()"
      />
    </div>
  `
})
export class SsoCreateFormComponent {
  form = input.required<SsoFormModel>();
  info = input<string>('');
  theme = input<'light' | 'dark'>('dark');

  /** At least 13 years old (domain rule). */
  maxDobIso(): string {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 13);
    return d.toISOString().slice(0, 10);
  }

  /** At most 120 years old (domain rule). */
  minDobIso(): string {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 120);
    return d.toISOString().slice(0, 10);
  }
}
