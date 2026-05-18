import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
  readonly error = signal('');
  readonly loading = signal(false);
  readonly success = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  ngOnInit() {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) this.form.patchValue({ email });
  }

  submit() {
    if (this.form.invalid || this.loading()) return;

    this.error.set('');
    this.success.set(false);
    this.loading.set(true);

    const email = this.form.getRawValue().email;

    this.auth.requestPasswordReset(email).subscribe({
      next: () => {
        this.success.set(true);
        this.notify.success('Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu.');
        this.loading.set(false);
      },
      error: (err: any) => {
        this.success.set(true);
        this.notify.info('Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu.');
        this.loading.set(false);
      }
    });
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
