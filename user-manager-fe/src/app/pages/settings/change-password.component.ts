import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { TranslateService } from '../../core/services/translate.service';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../core/utils/http-error.utils';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private notify = inject(NotificationService);
  themeService = inject(ThemeService);
  private pageToolbar = inject(PageToolbarService);
  private translate = inject(TranslateService);

  error = '';
  loading = false;
  success = false;
  theme = this.themeService.theme;
  showCurrent = false;
  showNew = false;
  showConfirm = false;

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Security',
      description: this.translate.t().changePasswordDesc
    });
  }

  form = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: (g) => (g.get('newPassword')?.value === g.get('confirmPassword')?.value ? null : { mismatch: true }) }
  );

  submit() {
    if (this.form.invalid || this.loading) return;
    this.error = '';
    this.loading = true;
    const { currentPassword, newPassword } = this.form.getRawValue();
    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.success = true;
        this.notify.success('Mật khẩu đã được đổi.');
        this.form.reset();
        this.loading = false;
      },
      error: (err) => {
        this.error = getHttpErrorMessage(err, 'Đổi mật khẩu thất bại.', UNREACHABLE_SERVER_MESSAGE_VI);
        this.notify.error(this.error);
        this.loading = false;
      }
    });
  }
}
