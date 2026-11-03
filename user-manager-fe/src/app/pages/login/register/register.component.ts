import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { apiErrorMessage } from '../../../core/utils/http-error.utils';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';

/** Mirrors the server's ASP.NET Identity password policy for immediate feedback. */
function passwordPolicy(control: AbstractControl): ValidationErrors | null {
  const v = String(control.value ?? '');
  const ok = v.length >= 8 && /[a-z]/.test(v) && /[A-Z]/.test(v) && /\d/.test(v) && /[^a-zA-Z0-9]/.test(v);
  return ok ? null : { policy: true };
}

/** Self-service registration. New accounts get the User role; administrators grant application access. */
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  template: `
    <app-auth-shell [wide]="true">
      <div class="mb-8">
        <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ t().registerTitle }}</h1>
        <p class="mt-1.5 text-sm text-fg-2">{{ t().registerSubtitle }}</p>
      </div>

      @if (done()) {
        <div class="alert-ok" role="status"><span class="material-symbols-outlined">mark_email_unread</span><span>{{ t().registerDone }}</span></div>
        <a routerLink="/sso-login" class="btn-primary btn-lg btn-block mt-4">{{ t().backToSignIn }}</a>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="grid grid-cols-1 gap-4 sm:grid-cols-2" novalidate>
          <div>
            <label class="label" for="rg-first">{{ t().registerFirstName }}</label>
            <input id="rg-first" class="input" formControlName="firstName" autocomplete="given-name" [class.input-invalid]="bad('firstName')" />
          </div>
          <div>
            <label class="label" for="rg-last">{{ t().registerLastName }}</label>
            <input id="rg-last" class="input" formControlName="lastName" autocomplete="family-name" [class.input-invalid]="bad('lastName')" />
          </div>
          <div class="sm:col-span-2">
            <label class="label" for="rg-email">{{ t().authEmail }}</label>
            <input id="rg-email" type="email" class="input" formControlName="email" autocomplete="email" [class.input-invalid]="bad('email')" />
            @if (bad('email')) { <p class="error-text">{{ t().authInvalidEmail }}</p> }
          </div>
          <div>
            <label class="label" for="rg-user">{{ t().registerUsername }}</label>
            <input id="rg-user" class="input" formControlName="userName" autocomplete="username" [class.input-invalid]="bad('userName')" />
          </div>
          <div>
            <label class="label" for="rg-dob">{{ t().registerDob }}</label>
            <input id="rg-dob" type="date" class="input" formControlName="dateOfBirth" autocomplete="bday" [class.input-invalid]="bad('dateOfBirth')" />
          </div>
          <div class="sm:col-span-2">
            <label class="label" for="rg-pass">{{ t().authPassword }}</label>
            <input id="rg-pass" type="password" class="input" formControlName="password" autocomplete="new-password" [class.input-invalid]="bad('password')" />
            <p [class]="bad('password') ? 'error-text' : 'hint'">{{ t().passwordPolicyHint }}</p>
          </div>
          @if (error()) {
            <div class="alert-bad sm:col-span-2" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
          }
          <button type="submit" class="btn-primary btn-lg btn-block sm:col-span-2" [disabled]="loading()">
            @if (loading()) { <span class="spinner"></span> {{ t().registerSubmitting }} } @else { {{ t().registerSubmit }} }
          </button>
        </form>
        <p class="mt-6 text-center text-[13px] text-fg-2">{{ t().registerHaveAccount }} <a routerLink="/sso-login" class="link">{{ t().authSignIn }}</a></p>
      }
    </app-auth-shell>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  readonly t = inject(TranslateService).t;

  readonly loading = signal(false);
  readonly error = signal('');
  readonly done = signal(false);
  private readonly submitted = signal(false);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    userName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    dateOfBirth: ['', Validators.required],
    password: ['', [Validators.required, passwordPolicy]]
  });

  bad(name: keyof typeof this.form.controls) {
    const c = this.form.controls[name];
    return c.invalid && (c.touched || this.submitted());
  }

  submit() {
    this.submitted.set(true);
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.register(this.form.getRawValue()).subscribe({
      next: res => {
        // Registration opens a session server-side; end it — access is granted after verification by an admin.
        if (res.data?.accessToken) {
          this.auth.setSession(res.data);
          this.auth.logout({ navigate: false });
        }
        this.loading.set(false);
        this.done.set(true);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(apiErrorMessage(err));
      }
    });
  }
}
