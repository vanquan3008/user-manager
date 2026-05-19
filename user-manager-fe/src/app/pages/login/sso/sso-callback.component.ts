import { Location } from '@angular/common';
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { hasAdminRole } from '../../../core/constants/roles';
import type { AuthResponse } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OauthService } from '../../../core/services/oauth.service';

@Component({
  selector: 'app-sso-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-slate-900 dark:to-slate-800">
      <div class="text-center">
        <div class="mb-4 inline-block h-16 w-16 animate-spin rounded-full border-4 border-blue-500 border-t-transparent"></div>
        <p class="text-lg font-medium text-slate-700 dark:text-slate-300">
          {{ message }}
        </p>
      </div>
    </div>
  `
})
export class SsoCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  private readonly oauth = inject(OauthService);
  private readonly authService = inject(AuthService);
  private readonly notify = inject(NotificationService);

  message = 'Đang xử lý đăng nhập SSO...';

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    const code = params.get('code');
    const state = params.get('state');
    const context = this.oauth.readAuthorizeContext();

    this.location.replaceState('/auth/callback');

    if (!code) {
      this.notify.error('Không tìm thấy authorization code.');
      void this.router.navigate(['/login'], { replaceUrl: true });
      return;
    }

    if (!context?.clientId || !context.redirectUri || !context.codeVerifier) {
      this.notify.error('SSO session context is missing or expired.');
      void this.router.navigate(['/login'], { replaceUrl: true });
      return;
    }

    this.message = 'Đang lấy access token...';

    this.oauth
      .exchangeToken({
        grantType: 'authorization_code',
        code,
        redirectUri: context.redirectUri,
        clientId: context.clientId,
        codeVerifier: context.codeVerifier
      })
      .subscribe({
        next: (res) => {
          const data = res.data;
          if (!res.success || !data?.accessToken) {
            this.notify.error(res.message || 'Không thể lấy access token.');
            void this.router.navigate(['/login'], { replaceUrl: true });
            return;
          }

          const authResponse: AuthResponse = {
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            expiresAt: new Date(Date.now() + (data.expiresIn || 3600) * 1000).toISOString(),
            user: data.user || {
              id: '',
              email: '',
              username: '',
              displayName: '',
              roles: [],
              permissions: []
            }
          };

          this.oauth.clearAuthorizeContext();
          this.authService.setSession(authResponse);

          if (!hasAdminRole(authResponse.user.roles)) {
            this.notify.error('Chỉ tài khoản Admin hoặc SuperAdmin mới được vào màn hình này.');
            this.authService.logout({ queryParams: { forbidden: '1' } });
            return;
          }

          this.notify.success('Đăng nhập SSO thành công.');
          const targetUrl = state?.startsWith('/') ? state : '/';
          void this.router.navigateByUrl(targetUrl, { replaceUrl: true });
        },
        error: (err: any) => {
          const errorMsg = err.error?.message || err.error?.error?.message || 'Đăng nhập SSO thất bại.';
          this.notify.error(errorMsg);
          void this.router.navigate(['/login'], { replaceUrl: true });
        }
      });
  }
}
