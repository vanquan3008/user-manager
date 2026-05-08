import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  private readonly location = inject(Location);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
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
      this.error.set('Invalid reset link. Missing token or email.');
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
        this.notify.success('Password updated. You can sign in now.');
        this.loading.set(false);
        setTimeout(() => this.router.navigate(['/login'], { replaceUrl: true }), 2000);
      },
      error: (err) => {
        this.error.set(getHttpErrorMessage(err, 'Password reset failed.', UNREACHABLE_SERVER_MESSAGE_VI));
        this.notify.error(this.error());
        this.loading.set(false);
      }
    });
  }

  toggleTheme() {
    this.themeService.toggleTheme();
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
