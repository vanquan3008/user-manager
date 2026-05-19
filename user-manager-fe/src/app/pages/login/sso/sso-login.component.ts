import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { environment } from '../../../../environments/environment';
import type { SsoAuthorizeRequest, SsoAuthorizeResponse, SsoConsentRequest } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OauthService } from '../../../core/services/oauth.service';
import { ThemeService } from '../../../core/services/theme.service';

type SsoLocale = 'vi' | 'en' | 'zh';

interface SsoTranslation {
  code: string;
  title: string;
  subtitle: string;
  emailLabel: string;
  emailPlaceholder: string;
  passwordLabel: string;
  passwordPlaceholder: string;
  loginButton: string;
  connecting: string;
  backToLogin: string;
  or: string;
  consentTitle: string;
  consentSubtitle: string;
  consentRequested: string;
  deny: string;
  approve: string;
  processing: string;
  errorFailed: string;
  errorCodeMissing: string;
  errorConsentFailed: string;
  errorDenied: string;
  securitySsl: string;
  securityOAuth: string;
}

const SSO_TRANSLATIONS: Record<SsoLocale, SsoTranslation> = {
  vi: {
    code: 'VI',
    title: 'SSO Enterprise',
    subtitle: 'Đăng nhập an toàn với tài khoản doanh nghiệp',
    emailLabel: 'Email SSO',
    emailPlaceholder: 'ten.ban@congty.com',
    passwordLabel: 'Mật khẩu SSO',
    passwordPlaceholder: '........',
    loginButton: 'Đăng nhập bảo mật',
    connecting: 'Đang kết nối SSO...',
    backToLogin: 'Quay lại đăng nhập thường',
    or: 'hoặc',
    consentTitle: 'Cấp quyền truy cập',
    consentSubtitle: 'muốn truy cập tài khoản của bạn',
    consentRequested: 'Ứng dụng này sẽ có thể:',
    deny: 'Từ chối',
    approve: 'Cho phép',
    processing: 'Đang xử lý...',
    errorFailed: 'Đăng nhập SSO thất bại',
    errorCodeMissing: 'Không nhận được authorization code',
    errorConsentFailed: 'Xác nhận quyền thất bại',
    errorDenied: 'Bạn đã từ chối cấp quyền',
    securitySsl: 'SSL/TLS Secure',
    securityOAuth: 'OAuth 2.0 Standard'
  },
  en: {
    code: 'EN',
    title: 'SSO Enterprise',
    subtitle: 'Secure sign-in with your business account',
    emailLabel: 'SSO Email',
    emailPlaceholder: 'your.name@company.com',
    passwordLabel: 'SSO Password',
    passwordPlaceholder: '........',
    loginButton: 'Secure Sign In',
    connecting: 'Connecting to SSO...',
    backToLogin: 'Back to standard login',
    or: 'or',
    consentTitle: 'Authorize Access',
    consentSubtitle: 'wants to access your account',
    consentRequested: 'This application will be able to:',
    deny: 'Deny',
    approve: 'Approve',
    processing: 'Processing...',
    errorFailed: 'SSO login failed',
    errorCodeMissing: 'Authorization code not received',
    errorConsentFailed: 'Consent verification failed',
    errorDenied: 'You have denied access',
    securitySsl: 'SSL/TLS Secure',
    securityOAuth: 'OAuth 2.0 Standard'
  },
  zh: {
    code: '中文',
    title: 'SSO Enterprise',
    subtitle: 'Secure sign-in with your business account',
    emailLabel: 'SSO Email',
    emailPlaceholder: 'name@company.com',
    passwordLabel: 'SSO Password',
    passwordPlaceholder: '........',
    loginButton: 'Secure Sign In',
    connecting: 'Connecting to SSO...',
    backToLogin: 'Back to standard login',
    or: 'or',
    consentTitle: 'Authorize Access',
    consentSubtitle: 'wants to access your account',
    consentRequested: 'This application will be able to:',
    deny: 'Deny',
    approve: 'Approve',
    processing: 'Processing...',
    errorFailed: 'SSO login failed',
    errorCodeMissing: 'Authorization code not received',
    errorConsentFailed: 'Consent verification failed',
    errorDenied: 'You have denied access',
    securitySsl: 'SSL/TLS Secure',
    securityOAuth: 'OAuth 2.0 Standard'
  }
};

@Component({
  selector: 'app-sso-login',
  standalone: true,
  host: { class: 'app-host-fill' },
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './sso-login.component.html'
})
export class SsoLoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);
  private readonly oauth = inject(OauthService);
  private readonly themeService = inject(ThemeService);

  readonly theme = this.themeService.theme;
  readonly error = signal('');
  readonly loading = signal(false);
  readonly showConsent = signal(false);
  readonly consentData = signal<SsoAuthorizeResponse | null>(null);
  readonly language = signal<SsoLocale>('vi');
  readonly showPassword = signal(false);

  readonly t = computed(() => SSO_TRANSLATIONS[this.language()]);

  readonly scopeDescriptions = computed(() => {
    const lang = this.language();
    return {
      profile: {
        title: lang === 'vi' ? 'Thông tin cá nhân' : 'Personal Profile',
        desc: lang === 'vi' ? 'Họ tên, ảnh đại diện và thông tin cơ bản' : 'Your name, avatar, and basic info',
        icon: 'person'
      },
      email: {
        title: lang === 'vi' ? 'Địa chỉ email' : 'Email Address',
        desc: lang === 'vi' ? 'Truy cập vào email chính của bạn' : 'Access to your primary email address',
        icon: 'mail'
      },
      openid: {
        title: lang === 'vi' ? 'Định danh số' : 'Digital Identity',
        desc: lang === 'vi' ? 'Xác thực định danh duy nhất của bạn' : 'Verify your unique digital identity',
        icon: 'fingerprint'
      },
      phone: {
        title: lang === 'vi' ? 'Số điện thoại' : 'Phone Number',
        desc: lang === 'vi' ? 'Xem số điện thoại đã xác thực' : 'View your verified phone number',
        icon: 'phone'
      },
      address: {
        title: lang === 'vi' ? 'Địa chỉ' : 'Address',
        desc: lang === 'vi' ? 'Xem địa chỉ liên lạc của bạn' : 'View your contact address',
        icon: 'map'
      }
    } as Record<string, { title: string; desc: string; icon: string }>;
  });

  readonly mappedScopes = computed(() => {
    const data = this.consentData();
    const descriptions = this.scopeDescriptions();
    if (!data?.requestedScopes?.length) {
      return [];
    }

    return data.requestedScopes.map(scope => ({
      key: scope,
      title: descriptions[scope]?.title || scope,
      desc: descriptions[scope]?.desc || 'Extended access',
      icon: descriptions[scope]?.icon || 'extension'
    }));
  });

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  private clientId = '';
  private redirectUri = '';
  private scopes: string[] = [];
  private state = '';
  private codeChallenge = '';
  private codeChallengeMethod = 'S256';
  private codeVerifier = '';
  private isExternalClientFlow = false;

  ngOnInit() {
    void this.initOAuthParams();
    this.initForm();
  }

  setLanguage(lang: SsoLocale) {
    this.language.set(lang);
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  togglePasswordVisibility() {
    this.showPassword.update(value => !value);
  }

  submit() {
    if (this.form.invalid || this.loading()) {
      return;
    }

    if (!this.clientId || !this.redirectUri || !this.codeChallenge) {
      this.error.set(
        this.isExternalClientFlow
          ? 'Missing PKCE code_challenge for this SSO request.'
          : 'Secure sign-in context is not ready yet. Please try again.'
      );
      return;
    }

    this.error.set('');
    this.loading.set(true);

    const { email, password } = this.form.getRawValue();
    const request: SsoAuthorizeRequest = {
      email,
      password,
      clientId: this.clientId,
      redirectUri: this.redirectUri,
      scopes: this.scopes.length > 0 ? this.scopes : ['profile'],
      state: this.state,
      codeChallenge: this.codeChallenge,
      codeChallengeMethod: this.codeChallengeMethod
    };

    this.auth
      .ssoAuthorize(request)
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.handleError(this.t().errorFailed);
            return;
          }

          if (res.data.requiresConsent) {
            this.consentData.set(res.data);
            this.showConsent.set(true);
            this.loading.set(false);
            return;
          }

          if (!res.data.code) {
            this.handleError(this.t().errorCodeMissing);
            return;
          }

          this.redirectWithCode(res.data.code, res.data.state);
        },
        error: (err: any) => {
          this.handleError(err.error?.error?.message || err.error?.message || this.t().errorFailed);
        }
      });
  }

  approveConsent() {
    if (this.form.invalid || this.loading()) {
      return;
    }

    this.error.set('');
    this.loading.set(true);

    const { email, password } = this.form.getRawValue();

    const body: SsoConsentRequest = {
      email,
      password,
      clientId: this.clientId,
      redirectUri: this.redirectUri,
      scopes: this.scopes,
      state: this.state,
      codeChallenge: this.codeChallenge,
      codeChallengeMethod: this.codeChallengeMethod,
      approve: true
    };

    this.auth
      .ssoConsent(body)
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data?.code) {
            this.handleError(this.t().errorConsentFailed);
            return;
          }

          this.redirectWithCode(res.data.code, res.data.state);
        },
        error: (err: any) => {
          this.handleError(err.error?.error?.message || err.error?.message || this.t().errorConsentFailed);
        }
      });
  }

  denyConsent() {
    this.error.set(this.t().errorDenied);
    this.showConsent.set(false);
  }

  private async initOAuthParams() {
    const params = this.route.snapshot.queryParamMap;
    const clientIdParam = params.get('client_id')?.trim() || '';
    const redirectUriParam = params.get('redirect_uri')?.trim() || '';
    const scopeParam = params.get('scope') || '';
    const scopesParam = params.get('scopes') || '';
    const codeChallengeParam = params.get('code_challenge')?.trim() || '';

    this.state = params.get('state') || '';
    this.codeChallengeMethod = params.get('code_challenge_method') || environment.oauth.codeChallengeMethod;

    let scopesFromParam: string[] = [];
    if (scopeParam) {
      scopesFromParam = scopeParam.split(/\s+/).map(scope => scope.trim()).filter(Boolean);
    } else if (scopesParam) {
      scopesFromParam = scopesParam.split(',').map(scope => scope.trim()).filter(Boolean);
    }

    this.isExternalClientFlow = !!clientIdParam && !!redirectUriParam;

    if (this.isExternalClientFlow) {
      this.clientId = clientIdParam;
      this.redirectUri = redirectUriParam;
      this.scopes = scopesFromParam.length > 0 ? scopesFromParam : environment.oauth.defaultScopes;
      this.codeChallenge = codeChallengeParam;

      if (!this.codeChallenge) {
        this.error.set('Missing PKCE code_challenge for external SSO client.');
      }

      return;
    }

    this.clientId = environment.oauth.clientId;
    this.redirectUri = environment.oauth.redirectUri?.trim() || `${window.location.origin}/auth/callback`;
    this.scopes = environment.oauth.defaultScopes;
    this.state = params.get('returnUrl')?.trim() || 'internal-sso';

    const pkceContext = await this.oauth.createPkceContext();
    this.codeVerifier = pkceContext.codeVerifier;
    this.codeChallenge = pkceContext.codeChallenge;
  }

  private initForm() {
    const email = this.route.snapshot.queryParamMap.get('email')?.trim();
    if (email) {
      this.form.patchValue({ email });
    }
  }

  private handleError(message: string) {
    this.error.set(message);
    this.notify.error(message);
    this.loading.set(false);
  }

  private redirectWithCode(code: string, state?: string) {
    const url = new URL(this.redirectUri);
    url.searchParams.set('code', code);
    if (state) {
      url.searchParams.set('state', state);
    }

    if (!this.isExternalClientFlow && this.codeVerifier) {
      this.oauth.saveAuthorizeContext({
        clientId: this.clientId,
        redirectUri: this.redirectUri,
        codeVerifier: this.codeVerifier
      });
    }

    this.notify.success('Redirecting...');
    window.location.assign(url.toString());
  }
}
