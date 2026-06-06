import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OauthService } from '../../../core/services/oauth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { hasAdminRole } from '../../../core/constants/roles';
import { getHttpErrorMessage } from '../../../core/utils/http-error.utils';
import { environment } from '../../../../environments/environment';

type LoginLocale = 'vi' | 'en' | 'zh';

interface LoginTranslation {
  code: string;
  title: string;
  subtitle: string;
  sideTitle: string;
  sideSubtitle: string;
  sideBullet1: string;
  sideBullet2: string;
  sideBullet3: string;
  passwordTab: string;
  magicTab: string;
  emailLabel: string;
  emailPlaceholder: string;
  passwordLabel: string;
  passwordPlaceholder: string;
  signIn: string;
  signingIn: string;
  magicIntro: string;
  magicButton: string;
  magicSending: string;
  magicDefaultMessage: string;
  errorEmailRequired: string;
  errorPasswordRequired: string;
  forgotPassword: string;
  socialOr: string;
  socialGoogle: string;
  socialFacebook: string;
  ssoButton: string;
  socialGoogleMsg: string;
  socialFacebookMsg: string;
  ssoMissingConfig: string;
  badge1: string;
  badge2: string;
  badge3: string;
  ssoCheckbox: string;
  errorCannotReachServer: string;
}

const LOGIN_TRANSLATIONS: Record<LoginLocale, LoginTranslation> = {
  vi: {
    code: 'VI',
    title: 'Đăng nhập',
    subtitle: 'Chỉ Admin hoặc SuperAdmin mới truy cập được không gian quản trị này.',
    sideTitle: 'Quản lý user gọn, rõ, dễ dùng',
    sideSubtitle: 'Theo dõi user, vai trò và quyền truy cập trong một workspace nhẹ và dễ đọc.',
    sideBullet1: 'Xem nhanh user, trạng thái và gói đang dùng.',
    sideBullet2: 'Mở chi tiết để chỉnh role, quyền và SSO khi cần.',
    sideBullet3: 'Giữ thao tác quản trị ngắn gọn nhưng vẫn đủ kiểm soát.',
    passwordTab: 'Mật khẩu',
    magicTab: 'Magic link',
    emailLabel: 'Email',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: 'Mật khẩu',
    passwordPlaceholder: '••••••••',
    signIn: 'Đăng nhập',
    signingIn: 'Đang đăng nhập...',
    magicIntro: 'Nhập email Admin hoặc SuperAdmin. Hệ thống sẽ gửi cho bạn một liên kết đăng nhập an toàn.',
    magicButton: 'Gửi link đăng nhập',
    magicSending: 'Đang gửi...',
    magicDefaultMessage: 'Nếu email hợp lệ, liên kết đăng nhập đã được gửi. Hãy kiểm tra hộp thư của bạn.',
    errorEmailRequired: 'Vui lòng nhập đúng định dạng email',
    errorPasswordRequired: 'Vui lòng nhập mật khẩu',
    forgotPassword: 'Quên mật khẩu?',
    socialOr: 'Hoặc tiếp tục với',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    ssoButton: 'Đăng nhập bằng SSO',
    socialGoogleMsg: 'Đăng nhập Google chưa được cấu hình. Vui lòng dùng mật khẩu hoặc email.',
    socialFacebookMsg: 'Đăng nhập Facebook chưa được cấu hình. Vui lòng dùng mật khẩu hoặc email.',
    ssoMissingConfig: 'SSO chưa được cấu hình đầy đủ. Kiểm tra OAuth clientId và redirectUri.',
    badge1: 'User rõ ràng',
    badge2: 'Role gọn',
    badge3: 'Quyền an toàn',
    ssoCheckbox: 'Tôi là tài khoản SSO',
    errorCannotReachServer: 'Không thể kết nối tới máy chủ. Hãy kiểm tra API và cấu hình môi trường.'
  },
  en: {
    code: 'EN',
    title: 'Sign in',
    subtitle: 'Only Admin or SuperAdmin accounts can access this workspace.',
    sideTitle: 'User management that feels lighter',
    sideSubtitle: 'Review users, roles and access from one cleaner workspace.',
    sideBullet1: 'Scan users, status and plan at a glance.',
    sideBullet2: 'Open details only when you need roles, access or SSO.',
    sideBullet3: 'Shorter admin flows, with the same control.',
    passwordTab: 'Password',
    magicTab: 'Magic link',
    emailLabel: 'Email',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: 'Password',
    passwordPlaceholder: '••••••••',
    signIn: 'Sign in',
    signingIn: 'Signing in...',
    magicIntro: 'Enter an Admin or SuperAdmin email and we will send a secure sign-in link.',
    magicButton: 'Send sign-in link',
    magicSending: 'Sending...',
    magicDefaultMessage: 'If the email is valid, a sign-in link has been sent. Please check your inbox.',
    errorEmailRequired: 'A valid email is required',
    errorPasswordRequired: 'Password is required',
    forgotPassword: 'Forgot password?',
    socialOr: 'Or continue with',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    ssoButton: 'Sign in with SSO',
    socialGoogleMsg: 'Google sign-in is not configured yet. Please use password or email sign-in.',
    socialFacebookMsg: 'Facebook sign-in is not configured yet. Please use password or email sign-in.',
    ssoMissingConfig: 'SSO is not fully configured. Check the OAuth clientId and redirectUri.',
    badge1: 'Clear users',
    badge2: 'Tidy roles',
    badge3: 'Safe access',
    ssoCheckbox: 'I am an SSO account',
    errorCannotReachServer: 'Cannot reach the server. Check that the API and environment configuration are available.'
  },
  zh: {
    code: '中文',
    title: '登录',
    subtitle: '只有 Admin 或 SuperAdmin 账户可以进入这个管理空间。',
    sideTitle: '更轻的用户管理体验',
    sideSubtitle: '在一个更清晰的工作区里查看用户、角色和访问权限。',
    sideBullet1: '快速查看用户、状态和套餐信息。',
    sideBullet2: '需要时再展开角色、权限和 SSO 细节。',
    sideBullet3: '流程更短，但管理控制依然完整。',
    passwordTab: '密码登录',
    magicTab: '邮件链接',
    emailLabel: '邮箱',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: '密码',
    passwordPlaceholder: '••••••••',
    signIn: '登录',
    signingIn: '正在登录...',
    magicIntro: '输入 Admin 或 SuperAdmin 邮箱，我们会发送安全登录链接。',
    magicButton: '发送登录链接',
    magicSending: '发送中...',
    magicDefaultMessage: '如果邮箱有效，登录链接已发送，请检查收件箱。',
    errorEmailRequired: '请输入有效邮箱',
    errorPasswordRequired: '请输入密码',
    forgotPassword: '忘记密码？',
    socialOr: '或者继续使用',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    ssoButton: '使用 SSO 登录',
    socialGoogleMsg: 'Google 登录尚未配置，请使用密码或邮件登录。',
    socialFacebookMsg: 'Facebook 登录尚未配置，请使用密码或邮件登录。',
    ssoMissingConfig: 'SSO 配置不完整，请检查 OAuth clientId 和 redirectUri。',
    badge1: '用户清晰',
    badge2: '角色精简',
    badge3: '访问安全',
    ssoCheckbox: '我是 SSO 账户',
    errorCannotReachServer: '无法连接到服务器，请检查 API 和环境配置。'
  }
};

@Component({
  selector: 'app-login',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly oauth = inject(OauthService);
  public readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notify = inject(NotificationService);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
  readonly error = signal('');
  readonly loading = signal(false);
  readonly forbiddenMessage = signal('');
  readonly mode = signal<'password' | 'magic'>('password');
  readonly magicMessage = signal('');
  readonly language = signal<LoginLocale>('vi');
  readonly showPassword = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  readonly magicForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  get t(): LoginTranslation {
    return LOGIN_TRANSLATIONS[this.language()];
  }

  ngOnInit() {
    this.oauth.clearAuthorizeContext();

    if (this.route.snapshot.queryParamMap.get('forbidden') === '1') {
      this.forbiddenMessage.set('Bạn không có quyền truy cập. Chỉ Admin hoặc SuperAdmin mới vào được.');
    }
  }

  setLanguage(lang: LoginLocale) {
    this.language.set(lang);
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  togglePasswordVisibility() {
    this.showPassword.update(value => !value);
  }

  onForgotPassword() {
    const email = this.form.getRawValue().email;
    this.router.navigate(['/forgot-password'], email ? { queryParams: { email } } : {});
  }

  loginWith(provider: 'google' | 'facebook') {
    const msg = provider === 'google' ? this.t.socialGoogleMsg : this.t.socialFacebookMsg;
    this.notify.info(msg);
  }

  async loginWithSso() {
    if (this.loading()) return;

    const clientId = environment.oauth.clientId.trim();
    const redirectUri = this.resolveOauthRedirectUri();
    if (!clientId || !redirectUri) {
      this.notify.error(this.t.ssoMissingConfig);
      return;
    }

    this.loading.set(true);
    this.error.set('');

    try {
      const { codeVerifier, codeChallenge } = await this.oauth.createPkceContext();
      const state = this.createOauthState();

      this.oauth.saveAuthorizeContext({
        clientId,
        clientSecret: environment.oauth.clientSecret?.trim() || null,
        redirectUri,
        codeVerifier,
        state,
        returnUrl: null
      });

      const authorizeUrl = this.oauth.buildHostedAuthorizeUrl({
        clientId,
        redirectUri,
        scopes: environment.oauth.defaultScopes.slice(),
        state,
        codeChallenge,
        codeChallengeMethod: environment.oauth.codeChallengeMethod as 'S256' | 'plain'
      });

      window.location.assign(authorizeUrl);
    } catch {
      this.loading.set(false);
      this.notify.error(this.t.errorCannotReachServer);
    }
  }

  submit() {
    if (this.form.invalid || this.loading()) return;

    this.error.set('');
    this.loading.set(true);

    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: (res) => {
        const authData = res.data;

        if (!authData?.accessToken || !authData?.user) {
          const msg = res.message || res.error?.message || 'Đăng nhập thất bại';
          this.error.set(msg);
          this.notify.error(msg);
          this.loading.set(false);
          return;
        }

        if (!hasAdminRole(authData.user.roles)) {
          this.error.set('Chỉ tài khoản Admin hoặc SuperAdmin mới được đăng nhập tại đây.');
          this.notify.error(this.error());
          this.loading.set(false);
          return;
        }

        this.auth.setSession(authData);
        this.notify.success('Đăng nhập thành công.');
        this.router.navigate(['/'], { replaceUrl: true });
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(
          getHttpErrorMessage(err, 'Đăng nhập thất bại', this.t.errorCannotReachServer)
        );
        this.notify.error(this.error());
        this.loading.set(false);
      }
    });
  }

  requestMagic() {
    if (this.magicForm.invalid || this.loading()) return;

    this.error.set('');
    this.magicMessage.set('');
    this.loading.set(true);

    const email = this.magicForm.getRawValue().email;

    this.auth.requestMagicLink(email).subscribe({
      next: (res) => {
        this.magicMessage.set(res.message || this.t.magicDefaultMessage);
        this.notify.info(this.t.magicDefaultMessage);
        this.loading.set(false);
      },
      error: () => {
        this.magicMessage.set(this.t.magicDefaultMessage);
        this.notify.info(this.t.magicDefaultMessage);
        this.loading.set(false);
      }
    });
  }

  private resolveOauthRedirectUri(): string {
    const configured = environment.oauth.redirectUri.trim();
    if (!configured) {
      return `${window.location.origin}/oauth/callback`;
    }

    const url = new URL(configured, window.location.origin);
    if (url.origin === window.location.origin && (url.pathname === '/' || url.pathname === '')) {
      url.pathname = '/oauth/callback';
    }

    return url.toString();
  }

  private createOauthState(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }

    return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
  }
}
