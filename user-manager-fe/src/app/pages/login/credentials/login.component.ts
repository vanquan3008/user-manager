import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ThemeService } from '../../../core/services/theme.service';
import { hasAdminRole } from '../../../core/constants/roles';
import { getHttpErrorMessage } from '../../../core/utils/http-error.utils';

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
  socialGoogleMsg: string;
  socialFacebookMsg: string;
  badge1: string;
  badge2: string;
  badge3: string;
  ssoCheckbox: string;
  errorCannotReachServer: string;
}

const LOGIN_TRANSLATIONS: Record<LoginLocale, LoginTranslation> = {
  vi: {
    code: 'VI',
    title: 'User Admin',
    subtitle: 'Chỉ Admin / SuperAdmin mới vào được giao diện quản trị này.',
    sideTitle: 'CRM cho đội ngũ social media',
    sideSubtitle: 'Tập trung toàn bộ user, phân quyền và gói dịch vụ trong một màn hình quản trị.',
    sideBullet1: 'Theo dõi quyền hạn và trạng thái truy cập theo thời gian thực.',
    sideBullet2: 'Phân bổ gói dịch vụ theo team nội bộ hoặc client.',
    sideBullet3: 'Đăng nhập an toàn bằng mật khẩu, email hoặc SSO (tùy chọn).',
    passwordTab: 'Đăng nhập bằng mật khẩu',
    magicTab: 'Đăng nhập bằng cách khác',
    emailLabel: 'Email',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: 'Mật khẩu',
    passwordPlaceholder: '••••••••',
    signIn: 'Đăng nhập',
    signingIn: 'Đang đăng nhập…',
    magicIntro: 'Nhập email tài khoản Admin/SuperAdmin. Hệ thống sẽ gửi liên kết xác thực qua email.',
    magicButton: 'Xác thực bằng email',
    magicSending: 'Đang gửi…',
    magicDefaultMessage: 'Đã gửi thành công, vui lòng kiểm tra hòm thư của bạn.',
    errorEmailRequired: 'Vui lòng nhập đúng định dạng email',
    errorPasswordRequired: 'Vui lòng nhập mật khẩu',
    forgotPassword: 'Quên mật khẩu?',
    socialOr: 'Hoặc tiếp tục with',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    socialGoogleMsg: 'Đăng nhập Google chưa được cấu hình. Vui lòng dùng mật khẩu hoặc liên kết xác thực email.',
    socialFacebookMsg: 'Đăng nhập Facebook chưa được cấu hình. Vui lòng dùng mật khẩu hoặc liên kết xác thực email.',
    badge1: 'Realtime roles',
    badge2: 'Plan control',
    badge3: 'Secure access',
    ssoCheckbox: 'Tôi là tài khoản SSO',
    errorCannotReachServer: 'Không thể kết nối tới máy chủ. Hãy kiểm tra API đã chạy và địa chỉ trong cấu hình.'
  },
  en: {
    code: 'EN',
    title: 'User Admin',
    subtitle: 'Only Admin / SuperAdmin accounts can access this console.',
    sideTitle: 'CRM for social media teams',
    sideSubtitle: 'Centralise users, roles and subscription plans for all your social channels.',
    sideBullet1: 'Monitor roles and access status in real time.',
    sideBullet2: 'Assign plans by internal teams or client accounts.',
    sideBullet3: 'Secure sign‑in via password, email link or SSO (optional).',
    passwordTab: 'Sign in with password',
    magicTab: 'Sign in another way',
    emailLabel: 'Email',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: 'Password',
    passwordPlaceholder: '••••••••',
    signIn: 'Sign in',
    signingIn: 'Signing in…',
    magicIntro: 'Enter an Admin / SuperAdmin email. We will send you a secure verification link.',
    magicButton: 'Verify via email',
    magicSending: 'Sending…',
    magicDefaultMessage: 'If this email exists and is an Admin / SuperAdmin, a verification link has been sent. Please check your inbox.',
    errorEmailRequired: 'A valid email is required',
    errorPasswordRequired: 'Password is required',
    forgotPassword: 'Forgot password?',
    socialOr: 'Or continue with',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    socialGoogleMsg: 'Google sign-in is not configured yet. Please use password or email verification.',
    socialFacebookMsg: 'Facebook sign-in is not configured yet. Please use password or email verification.',
    badge1: 'Realtime roles',
    badge2: 'Plan control',
    badge3: 'Secure access',
    ssoCheckbox: 'I am an SSO account',
    errorCannotReachServer: 'Cannot reach the server. Check that the API is running and the URL in configuration is correct.'
  },
  zh: {
    code: '中文',
    title: '用户管理后台',
    subtitle: '只有 Admin / SuperAdmin 账号可以访问此管理界面。',
    sideTitle: '为社交媒体团队打造的 CRM',
    sideSubtitle: '在同一个后台集中管理用户、角色权限和套餐。',
    sideBullet1: '实时查看角色和访问状态。',
    sideBullet2: '按内部团队或客户账号分配套餐。',
    sideBullet3: '通过密码、邮箱验证链接或 SSO（可选）安全登录。',
    passwordTab: '密码登录',
    magicTab: '其他登录方式',
    emailLabel: '邮箱',
    emailPlaceholder: 'admin@example.com',
    passwordLabel: '密码',
    passwordPlaceholder: '••••••••',
    signIn: '登录',
    signingIn: '正在登录…',
    magicIntro: '输入 Admin / SuperAdmin 邮箱，我们会发送一封邮箱验证链接。',
    magicButton: '邮箱验证登录',
    magicSending: '正在发送…',
    magicDefaultMessage: '如果该邮箱存在并且是 Admin / SuperAdmin，我们已发送验证链接，请检查邮箱。',
    errorEmailRequired: '请输入有效的邮箱地址',
    errorPasswordRequired: '请输入密码',
    forgotPassword: '忘记密码？',
    socialOr: '或使用以下方式继续',
    socialGoogle: 'Google',
    socialFacebook: 'Facebook',
    socialGoogleMsg: '暂未配置 Google 登录，请先使用密码或邮箱验证登录。',
    socialFacebookMsg: '暂未配置 Facebook 登录，请先使用密码或邮箱验证登录。',
    badge1: '实时角色',
    badge2: '套餐管理',
    badge3: '安全访问',
    ssoCheckbox: '我是 SSO 账户',
    errorCannotReachServer: '无法连接到服务器。请确认 API 已启动且配置地址正确。'
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
  private returnUrl: string | null = null;

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
    if (this.route.snapshot.queryParamMap.get('forbidden') === '1') {
      this.forbiddenMessage.set('Bạn không có quyền truy cập. Chỉ Admin hoặc SuperAdmin mới được vào.');
    }
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
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

  gotoSsoLogin() {
    this.navigateToSsoLogin(this.form.getRawValue().email);
  }

  submit() {
    if (this.form.invalid || this.loading()) return;

    this.error.set('');
    this.loading.set(true);

    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password, { returnUrl: this.returnUrl }).subscribe({
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
        if (this.returnUrl?.startsWith('/')) {
          this.router.navigateByUrl(this.returnUrl, { replaceUrl: true });
        } else {
          this.router.navigate(['/'], { replaceUrl: true });
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        if (this.isSsoLoginRequired(err)) {
          this.loading.set(false);
          this.navigateToSsoLogin(email);
          return;
        }

        this.error.set(
          getHttpErrorMessage(err, 'Đăng nhập thất bại', this.t.errorCannotReachServer)
        );
        this.notify.error(this.error());
        this.loading.set(false);
      }
    });
  }

  private navigateToSsoLogin(email?: string): void {
    const queryParams: Record<string, string> = {};
    const trimmedEmail = email?.trim();
    const trimmedReturnUrl = this.returnUrl?.trim();

    if (trimmedEmail) {
      queryParams['email'] = trimmedEmail;
    }

    if (trimmedReturnUrl) {
      queryParams['returnUrl'] = trimmedReturnUrl;
    }

    this.router.navigate(['/sso-login'], { queryParams, replaceUrl: true });
  }

  private isSsoLoginRequired(err: unknown): boolean {
    if (!(err instanceof HttpErrorResponse)) {
      return false;
    }

    return err.error?.error?.code === 'SsoLoginRequiredException';
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
      error: (err: any) => {
        this.magicMessage.set(this.t.magicDefaultMessage);
        this.notify.info(this.t.magicDefaultMessage);
        this.loading.set(false);
      }
    });
  }
}
