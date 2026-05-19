import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);
  private readonly location = inject(Location);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
  readonly error = signal('');
  readonly loading = signal(true);
  readonly success = signal(false);

  ngOnInit() {
    const params = this.readLinkParams();
    const token = params.get('token');

    this.clearSensitiveUrlState();

    if (!token) {
      this.error.set('Invalid verification link.');
      this.loading.set(false);
      return;
    }

    this.auth.verifyEmail(token).subscribe({
      next: (res: any) => {
        this.success.set(res.success !== false);
        this.notify.success(this.success() ? 'Email verified successfully.' : res.message || 'Verification handled.');
        this.loading.set(false);
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err: any) => {
        this.error.set(getHttpErrorMessage(err, 'Email verification failed.', UNREACHABLE_SERVER_MESSAGE_VI));
        this.notify.error(this.error());
        this.loading.set(false);
        setTimeout(() => this.router.navigate(['/login']), 2000);
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
    if (queryToken) params.set('token', queryToken);
    return params;
  }

  private clearSensitiveUrlState(): void {
    this.location.replaceState('/verify-email');
  }
}
