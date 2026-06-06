import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OauthService } from '../../../core/services/oauth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { getHttpErrorMessage } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-sso-login',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sso-login.component.html'
})
export class SsoLoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  private readonly oauth = inject(OauthService);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
  readonly loading = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);
  private returnUrl: string | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(value => !value);
  }

  useAdminLogin(): void {
    this.oauth.clearAuthorizeContext();
    this.router.navigate(['/login'], { replaceUrl: true });
  }

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.error.set('');
    this.loading.set(true);

    const { email, password } = this.form.getRawValue();
    this.auth.ssoLogin(email, password).subscribe({
      next: (res) => {
        const authData = res.data;
        if (!authData?.accessToken || !authData?.user?.userId) {
          const msg = res.error?.message || res.message || 'SSO login failed.';
          this.error.set(msg);
          this.notify.error(msg);
          this.loading.set(false);
          return;
        }

        this.auth.setHostedSsoSession(authData);
        const nextUrl = this.returnUrl?.startsWith('/') ? this.returnUrl : '/';
        this.router.navigateByUrl(nextUrl, { replaceUrl: true });
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(getHttpErrorMessage(err, 'SSO login failed.', 'Cannot reach the server.'));
        this.notify.error(this.error());
        this.loading.set(false);
      }
    });
  }
}
