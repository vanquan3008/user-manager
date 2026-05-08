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
    const userId = params.get('userId');

    this.clearSensitiveUrlState();

    if (!token) {
      this.error.set('Invalid verification link.');
      this.loading.set(false);
      return;
    }

    this.auth.verifyEmail(token, userId ?? undefined).subscribe({
      next: (res) => {
        this.success.set(res.success !== false);
        this.notify.success(this.success() ? 'Email verified successfully.' : res.message || 'Verification handled.');
        this.loading.set(false);
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err) => {
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
    const queryUserId = this.route.snapshot.queryParamMap.get('userId');
    if (queryToken) params.set('token', queryToken);
    if (queryUserId) params.set('userId', queryUserId);
    return params;
  }

  private clearSensitiveUrlState(): void {
    this.location.replaceState('/verify-email');
  }
}
