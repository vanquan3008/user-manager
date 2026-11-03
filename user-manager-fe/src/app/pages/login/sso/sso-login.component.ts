import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OauthService } from '../../../core/services/oauth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { apiErrorCode, apiErrorMessage } from '../../../core/utils/http-error.utils';
import type { HostedSsoLoginResponse } from '../../../core/models';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';
import { MfaStepComponent } from '../../../shared/components/mfa-step/mfa-step.component';

/** Hosted sign-in used by the OAuth authorize flow for SSO (end-user) accounts. */
@Component({
  selector: 'app-sso-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent, MfaStepComponent],
  template: `
    <app-auth-shell>
      @if (mfa(); as pending) {
        <app-mfa-step [email]="pending.email" [loading]="loading()" [error]="error()" (verify)="verifyMfa($event)" (back)="mfa.set(null)" />
      } @else {
        <div class="mb-8">
          <h1 class="text-[26px] font-semibold leading-tight tracking-[-0.02em]">{{ t().ssoPortalTitle }}</h1>
          <p class="mt-1.5 text-sm text-fg-2">{{ t().ssoPortalSubtitle }}</p>
          @if (returnUrl) {
            <p class="mt-3 inline-flex items-center gap-1.5 text-xs text-fg-3"><span class="material-symbols-outlined text-[15px]">subdirectory_arrow_right</span>{{ t().ssoContinuingTo }}</p>
          }
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4" novalidate>
          <div>
            <label class="label" for="sso-email">{{ t().authEmail }}</label>
            <input id="sso-email" type="email" class="input input-lg" formControlName="email" autocomplete="username" placeholder="you@company.com"
                   [class.input-invalid]="invalid('email')" autofocus />
            @if (invalid('email')) { <p class="error-text">{{ t().authInvalidEmail }}</p> }
          </div>
          <div>
            <div class="mb-1.5 flex items-center justify-between">
              <label class="text-[13px] font-medium" for="sso-password">{{ t().authPassword }}</label>
              <a routerLink="/forgot-password" class="link text-[13px]">{{ t().authForgot }}</a>
            </div>
            <div class="relative">
              <input id="sso-password" class="input input-lg pr-10" formControlName="password" autocomplete="current-password"
                     [type]="showPassword() ? 'text' : 'password'" [class.input-invalid]="invalid('password')" />
              <button type="button" class="btn-icon-sm absolute right-1 top-1" (click)="showPassword.set(!showPassword())"
                      [attr.aria-label]="showPassword() ? t().authHidePassword : t().authShowPassword">
                <span class="material-symbols-outlined text-[18px]">{{ showPassword() ? 'visibility_off' : 'visibility' }}</span>
              </button>
            </div>
            @if (invalid('password')) { <p class="error-text">{{ t().authPasswordRequired }}</p> }
          </div>

          @if (error()) {
            <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() }}</span></div>
          }

          <button type="submit" class="btn-primary btn-lg btn-block" [disabled]="loading()">
            @if (loading()) { <span class="spinner"></span> {{ t().authSigningIn }} } @else { {{ t().authSignIn }} }
          </button>
        </form>

        <div class="mt-6 flex items-center justify-between text-[13px]">
          <a routerLink="/register" class="link">{{ t().registerLink }}</a>
          <button type="button" class="font-medium text-fg-2 hover:text-fg" (click)="useAdminLogin()">{{ t().authConsoleTitle }}</button>
        </div>
      }
    </app-auth-shell>
  `
})
export class SsoLoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly oauth = inject(OauthService);
  readonly t = inject(TranslateService).t;

  readonly loading = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);
  readonly submitted = signal(false);
  readonly mfa = signal<{ token: string; email: string } | null>(null);
  returnUrl: string | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  ngOnInit(): void {
    const value = this.route.snapshot.queryParamMap.get('returnUrl');
    // Only same-app relative paths are accepted (no protocol-relative or absolute URLs).
    this.returnUrl = value && value.startsWith('/') && !value.startsWith('//') ? value : null;
  }

  invalid(control: 'email' | 'password') {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || this.submitted());
  }

  useAdminLogin(): void {
    this.oauth.clearAuthorizeContext();
    this.router.navigate(['/login'], { replaceUrl: true });
  }

  submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.loading()) return;
    this.error.set('');
    this.loading.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.ssoLogin(email, password).subscribe({
      next: res => this.complete(res.data, email),
      error: err => this.fail(err)
    });
  }

  verifyMfa(code: string): void {
    const pending = this.mfa();
    if (!pending) return;
    this.error.set('');
    this.loading.set(true);
    this.auth.verifySsoMfa(pending.token, code).subscribe({
      next: res => this.complete(res.data, pending.email),
      error: err => {
        if (apiErrorCode(err) === 'INVALID_MFA_CHALLENGE') this.mfa.set(null);
        this.fail(err);
      }
    });
  }

  private complete(data: HostedSsoLoginResponse | null, email: string) {
    if (data?.mfaRequired && data.mfaToken) {
      this.loading.set(false);
      this.mfa.set({ token: data.mfaToken, email: data.user?.email || email });
      return;
    }
    if (!data?.accessToken || !data.user?.userId) {
      this.fail(null);
      return;
    }
    this.auth.setHostedSsoSession(data);
    this.router.navigateByUrl(this.returnUrl ?? '/', { replaceUrl: true });
  }

  private fail(err: unknown) {
    this.loading.set(false);
    this.error.set(err ? apiErrorMessage(err, this.t().authFailed) : this.t().authFailed);
  }
}
