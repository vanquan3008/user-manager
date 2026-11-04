import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OauthService } from '../../../core/services/oauth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { hasAdminRole } from '../../../core/constants/roles';
import { apiErrorCode, apiErrorMessage } from '../../../core/utils/http-error.utils';
import type { AuthResponse } from '../../../core/models';
import { environment } from '../../../../environments/environment';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';
import { MfaStepComponent } from '../../../shared/components/mfa-step/mfa-step.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent, MfaStepComponent],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  readonly t = inject(TranslateService).t;

  readonly error = signal('');
  readonly notice = signal('');
  readonly loading = signal(false);
  readonly mode = signal<'password' | 'magic'>('password');
  readonly magicSent = signal(false);
  readonly showPassword = signal(false);
  readonly submitted = signal(false);
  /** Pending second factor after a correct password. */
  readonly mfa = signal<{ token: string; email: string } | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  readonly magicForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  ngOnInit() {
    this.oauth.clearAuthorizeContext();
    const q = this.route.snapshot.queryParamMap;
    if (q.get('forbidden') === '1') this.notice.set(this.t().authAdminOnly);
    else if (q.get('expired') === '1') this.notice.set(this.t().authSessionExpired);
  }

  setMode(mode: 'password' | 'magic') {
    this.mode.set(mode);
    this.error.set('');
    this.magicSent.set(false);
    const email = (mode === 'magic' ? this.form.controls.email : this.magicForm.controls.email).value;
    (mode === 'magic' ? this.magicForm.controls.email : this.form.controls.email).setValue(email);
  }

  invalid(control: 'email' | 'password') {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || this.submitted());
  }

  submit() {
    this.submitted.set(true);
    if (this.form.invalid || this.loading()) return;

    this.error.set('');
    this.loading.set(true);
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: res => this.handleAuthResult(res.data, email),
      error: err => this.fail(err)
    });
  }

  verifyMfa(code: string) {
    const pending = this.mfa();
    if (!pending || this.loading()) return;
    this.error.set('');
    this.loading.set(true);
    this.auth.verifyMfa(pending.token, code).subscribe({
      next: res => this.handleAuthResult(res.data, pending.email),
      error: err => {
        if (apiErrorCode(err) === 'INVALID_MFA_CHALLENGE') {
          this.mfa.set(null);
          this.notice.set(this.t().mfaExpired);
        }
        this.fail(err);
      }
    });
  }

  cancelMfa() {
    this.mfa.set(null);
    this.error.set('');
    this.form.controls.password.reset('');
  }

  requestMagic() {
    if (this.magicForm.invalid || this.loading()) {
      this.magicForm.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set('');
    // The API answers identically whether or not the account exists.
    this.auth.requestMagicLink(this.magicForm.getRawValue().email).subscribe({
      next: () => this.magicDone(),
      error: () => this.magicDone()
    });
  }

  async loginWithSso() {
    if (this.loading()) return;
    const clientId = environment.oauth.clientId.trim();
    const redirectUri = this.resolveOauthRedirectUri();
    if (!clientId || !redirectUri) {
      this.notify.error(this.t().authSsoMissingConfig);
      return;
    }

    this.loading.set(true);
    try {
      const { codeVerifier, codeChallenge } = await this.oauth.createPkceContext();
      const state = crypto.randomUUID();
      const nonce = crypto.randomUUID();
      this.oauth.saveAuthorizeContext({ clientId, redirectUri, codeVerifier, state, nonce, returnUrl: null });
      window.location.assign(this.oauth.buildHostedAuthorizeUrl({
        clientId,
        redirectUri,
        scopes: environment.oauth.defaultScopes.slice(),
        state,
        nonce,
        codeChallenge,
        codeChallengeMethod: 'S256'
      }));
    } catch (err) {
      this.loading.set(false);
      this.fail(err);
    }
  }

  private handleAuthResult(data: AuthResponse | null, email: string) {
    if (data?.mfaRequired && data.mfaToken) {
      this.loading.set(false);
      this.mfa.set({ token: data.mfaToken, email: data.user?.email || email });
      return;
    }

    if (!data?.accessToken || !data.user) {
      this.fail(null);
      return;
    }

    if (!hasAdminRole(data.user.roles)) {
      // Close the server session that was just opened for a non-administrator.
      this.auth.setSession(data);
      this.auth.logout({ queryParams: { forbidden: '1' } });
      this.mfa.set(null);
      this.loading.set(false);
      this.error.set(this.t().authAdminOnly);
      return;
    }

    this.auth.setSession(data);
    this.notify.success(this.t().authSignedIn);
    this.router.navigate(['/'], { replaceUrl: true });
  }

  private magicDone() {
    this.loading.set(false);
    this.magicSent.set(true);
  }

  private fail(err: unknown) {
    this.loading.set(false);
    this.error.set(err ? apiErrorMessage(err, this.t().authFailed) : this.t().authFailed);
  }

  private resolveOauthRedirectUri(): string {
    const configured = environment.oauth.redirectUri.trim();
    if (!configured) return `${window.location.origin}/oauth/callback`;
    const url = new URL(configured, window.location.origin);
    if (url.origin === window.location.origin && (url.pathname === '/' || url.pathname === '')) url.pathname = '/oauth/callback';
    return url.toString();
  }
}
