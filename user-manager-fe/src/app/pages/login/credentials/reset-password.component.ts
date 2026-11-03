import { Component, OnInit, inject, signal } from '@angular/core';
import { Location } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslateService } from '../../../core/services/translate.service';
import { apiErrorMessage } from '../../../core/utils/http-error.utils';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  template: `
    <app-auth-shell>
      <div class="mb-8">
        <div class="mb-5 flex h-11 w-11 items-center justify-center rounded-xl bg-brand-soft text-brand-text"><span class="material-symbols-outlined">password</span></div>
        <h1 class="text-2xl font-semibold tracking-tight">{{ t().resetTitle }}</h1>
        <p class="mt-1.5 text-sm text-fg-2">{{ t().resetSubtitle }}</p>
        @if (email()) { <p class="mt-3 inline-flex rounded-md bg-surface-2 px-2 py-1 text-xs font-medium text-fg-2">{{ email() }}</p> }
      </div>

      @if (success()) {
        <div class="alert-ok" role="status"><span class="material-symbols-outlined">check_circle</span><span>{{ t().resetDone }}</span></div>
        <a routerLink="/login" class="btn-primary btn-lg btn-block mt-4">{{ t().authSignIn }}</a>
      } @else if (!token() || !email()) {
        <div class="alert-bad" role="alert"><span class="material-symbols-outlined">link_off</span><span>{{ t().resetInvalidLink }}</span></div>
        <a routerLink="/forgot-password" class="btn-secondary btn-block mt-4">{{ t().forgotSubmit }}</a>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
          <div>
            <label class="label" for="rp-new">{{ t().resetNewPassword }}</label>
            <input id="rp-new" type="password" class="input input-lg" formControlName="newPassword" autocomplete="new-password" autofocus
                   [class.input-invalid]="form.controls.newPassword.invalid && form.controls.newPassword.touched" />
            <p class="hint">{{ t().passwordPolicyHint }}</p>
          </div>
          <div>
            <label class="label" for="rp-confirm">{{ t().resetConfirmPassword }}</label>
            <input id="rp-confirm" type="password" class="input input-lg" formControlName="confirmPassword" autocomplete="new-password"
                   [class.input-invalid]="form.hasError('mismatch') && form.controls.confirmPassword.touched" />
            @if (form.hasError('mismatch') && form.controls.confirmPassword.touched) { <p class="error-text">{{ t().resetMismatch }}</p> }
          </div>
          @if (error()) { <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div> }
          <button type="submit" class="btn-primary btn-lg btn-block" [disabled]="loading() || form.invalid">
            @if (loading()) { <span class="spinner"></span> } {{ t().resetSubmit }}
          </button>
        </form>
      }
    </app-auth-shell>
  `
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  private readonly location = inject(Location);
  readonly t = inject(TranslateService).t;
  readonly error = signal('');
  readonly loading = signal(false);
  readonly success = signal(false);
  readonly token = signal('');
  readonly email = signal('');

  readonly form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    {
      validators: (g) =>
        g.get('newPassword')?.value === g.get('confirmPassword')?.value ? null : { mismatch: true }
    }
  );

  ngOnInit() {
    const params = this.readLinkParams();
    this.token.set(params.get('token') ?? '');
    this.email.set(params.get('email') ?? '');

    this.clearSensitiveUrlState();

    if (!this.token() || !this.email()) {
      this.error.set('');
    }
  }

  submit() {
    if (this.form.invalid || this.loading() || !this.token() || !this.email()) return;

    this.error.set('');
    this.loading.set(true);

    const { newPassword } = this.form.getRawValue();

    this.auth.resetPassword(this.email(), this.token(), newPassword).subscribe({
      next: () => {
        this.success.set(true);
        this.notify.success(this.t().resetDone);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err, this.t().resetInvalidLink));
        this.loading.set(false);
      }
    });
  }


  private readLinkParams(): URLSearchParams {
    const fragment = this.route.snapshot.fragment?.trim();
    if (fragment) {
      return new URLSearchParams(fragment);
    }

    const params = new URLSearchParams();
    const queryToken = this.route.snapshot.queryParamMap.get('token');
    const queryEmail = this.route.snapshot.queryParamMap.get('email');
    const queryPurpose = this.route.snapshot.queryParamMap.get('purpose');
    if (queryToken) params.set('token', queryToken);
    if (queryEmail) params.set('email', queryEmail);
    if (queryPurpose) params.set('purpose', queryPurpose);
    return params;
  }

  private clearSensitiveUrlState(): void {
    this.location.replaceState('/reset-password');
  }
}
