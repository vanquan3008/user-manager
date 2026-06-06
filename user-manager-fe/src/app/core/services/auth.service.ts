import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { hasAdminRole } from '../constants/roles';
import type { ApiResponse, SuccessResponse } from '../models/http.model';
import type {
  AuthResponse,
  AuthenticatedUser,
  HostedSsoLoginResponse,
  SsoApplication,
  SsoExchangeTokenRequest
} from '../models';
import { SessionStorageService } from './session-storage.service';

const TOKEN_KEY = 'access_token';
const USER_KEY = 'user';
const APPLICATIONS_KEY = 'sso_applications';
const SESSION_KIND_KEY = 'session_kind';

type SessionKind = 'app' | 'hosted_sso';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiUrl}/api/auth`;
  private readonly clientId = environment.oauth.clientId;
  private readonly tokenSignal = signal<string | null>(null);
  private readonly userSignal = signal<AuthenticatedUser | null>(null);
  private readonly applicationsSignal = signal<SsoApplication[]>([]);
  private readonly sessionKindSignal = signal<SessionKind | null>(null);

  readonly isAuthenticated = computed(() => this.hasAppSession());
  readonly user = computed(() => this.userSignal());
  readonly isAdmin = computed(() => hasAdminRole(this.userSignal()?.roles));
  readonly permissions = computed(() => this.userSignal()?.permissions ?? []);
  readonly token = computed(() => this.tokenSignal());
  readonly applications = computed(() => this.applicationsSignal());
  readonly sessionKind = computed(() => this.sessionKindSignal());

  constructor(
    private http: HttpClient,
    private router: Router,
    private sessionStorage: SessionStorageService
  ) {
    this.tokenSignal.set(this.readAccessToken());
    this.userSignal.set(this.readUser());
    this.applicationsSignal.set(this.readApplications());
    this.sessionKindSignal.set(this.readSessionKind());
  }

  isTokenValid(): boolean {
    const token = this.readAccessToken();
    if (!token) {
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp as number | undefined;
      if (!exp) {
        return true;
      }

      return Date.now() / 1000 < exp - 60;
    } catch {
      return false;
    }
  }

  hasValidSession(): boolean {
    return this.isTokenValid() && !!this.userSignal();
  }

  hasAppSession(): boolean {
    return this.hasValidSession() && this.sessionKindSignal() === 'app';
  }

  hasHostedAuthSession(): boolean {
    return this.hasValidSession();
  }

  isHostedSsoSession(): boolean {
    return this.hasValidSession() && this.sessionKindSignal() === 'hosted_sso';
  }

  getAccessToken(): string | null {
    return this.tokenSignal();
  }

  refreshToken() {
    if (!this.userSignal()) {
      return throwError(() => new Error('No active session'));
    }

    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/refresh`, {}, { withCredentials: true });
  }

  register(data: {
    email: string;
    password: string;
    userName: string;
    firstName: string;
    lastName: string;
    dateOfBirth: string;
  }) {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/register`, {
      ...data,
      clientId: this.clientId
    });
  }

  login(
    email: string,
    password: string,
    options?: { returnUrl?: string | null }
  ) {
    const body: { email: string; password: string; clientId: string; returnUrl?: string } = {
      email,
      password,
      clientId: this.clientId
    };
    if (options?.returnUrl?.trim()) {
      body.returnUrl = options.returnUrl.trim();
    }

    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/login`, body);
  }

  ssoLogin(email: string, password: string) {
    return this.http.post<ApiResponse<HostedSsoLoginResponse>>(`${this.api}/sso/login`, {
      email,
      password
    });
  }

  ssoExchangeToken(request: SsoExchangeTokenRequest) {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/token-exchange`, request, {
      withCredentials: true
    });
  }

  requestMagicLink(email: string) {
    return this.http.post<ApiResponse<{ success: boolean }>>(`${this.api}/passwordless/request`, {
      email,
      clientId: this.clientId
    });
  }

  requestPasswordReset(email: string) {
    return this.http.post<ApiResponse<{ success: boolean }>>(`${this.api}/password/reset-request`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/password/reset`, { email, token, newPassword });
  }

  verifyEmail(token: string) {
    return this.http.get<ApiResponse<SuccessResponse>>(`${this.api}/verify-email`, { params: { token } });
  }

  resendMagicLink(email: string, purpose = 'EmailVerification', clientId?: string | null) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/magic-link/resend`, {
      email,
      purpose,
      clientId: clientId ?? this.clientId
    });
  }

  changePassword(currentPassword: string, newPassword: string) {
    return this.http.post<ApiResponse<SuccessResponse>>(`${this.api}/password/change`, { currentPassword, newPassword });
  }

  verifyMagicLink(token: string, purpose: string, clientId?: string) {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.api}/magic-link/verify`, {
      token,
      purpose,
      clientId: clientId ?? this.clientId
    });
  }

  hasPermission(code: string): boolean {
    const p = this.permissions();
    if (!p.length) return false;
    return p.some(x => x.toLowerCase() === code.toLowerCase());
  }

  hasAnyPermission(codes: string[]): boolean {
    const p = this.permissions();
    if (!p.length) return false;
    const set = new Set(p.map(x => x.toLowerCase()));
    return codes.some(c => set.has(c.toLowerCase()));
  }

  setSession(res: AuthResponse) {
    const applications = (res.applications ?? []).map((app) => this.normalizeApplication(app));
    this.applySession(
      res.accessToken,
      {
        id: res.user.id,
        email: res.user.email,
        username: res.user.username ?? res.user.email,
        displayName: res.user.displayName,
        roles: res.user.roles ?? [],
        permissions: res.user.permissions ?? []
      },
      applications,
      'app'
    );
  }

  setHostedSsoSession(res: HostedSsoLoginResponse) {
    this.applySession(
      res.accessToken,
      {
        id: res.user.userId,
        email: res.user.email,
        username: res.user.username ?? res.user.email,
        displayName: res.user.displayName,
        roles: res.user.roles ?? [],
        permissions: res.user.permissions ?? []
      },
      [],
      'hosted_sso'
    );
  }

  private applySession(
    accessToken: string,
    user: AuthenticatedUser,
    applications: SsoApplication[],
    sessionKind: SessionKind
  ) {
    this.sessionStorage.set(TOKEN_KEY, accessToken);
    this.tokenSignal.set(accessToken);

    this.sessionStorage.setJson(USER_KEY, user);
    this.userSignal.set(user);

    this.sessionStorage.set(SESSION_KIND_KEY, sessionKind);
    this.sessionKindSignal.set(sessionKind);

    if (applications.length) {
      this.sessionStorage.setJson(APPLICATIONS_KEY, applications);
      this.applicationsSignal.set(applications);
    } else {
      this.sessionStorage.remove(APPLICATIONS_KEY);
      this.applicationsSignal.set([]);
    }
  }

  private normalizeApplication(
    app:
      | SsoApplication
      | {
          applicationId?: string;
          applicationName?: string;
          role?: string;
        }
  ): SsoApplication {
    if ('id' in app && 'name' in app) {
      return app;
    }

    return {
      id: app.applicationId ?? '',
      name: app.applicationName ?? '',
      role: app.role
    };
  }

  logout(options?: { queryParams?: Record<string, string> }) {
    const token = this.tokenSignal();
    this.http.post<ApiResponse<SuccessResponse>>(
      `${this.api}/logout`,
      {},
      token ? { headers: { Authorization: `Bearer ${token}` } } : {}
    ).subscribe({ error: () => {} });

    this.clearSession();
    this.router.navigate(['/login'], { queryParams: options?.queryParams });
  }

  discardSession() {
    this.clearSession();
  }

  private readAccessToken(): string | null {
    return this.sessionStorage.get(TOKEN_KEY);
  }

  private readUser(): AuthenticatedUser | null {
    return this.sessionStorage.getJson<AuthenticatedUser | null>(USER_KEY, null);
  }

  private readApplications(): SsoApplication[] {
    return this.sessionStorage.getJson<SsoApplication[]>(APPLICATIONS_KEY, []);
  }

  private readSessionKind(): SessionKind | null {
    const value = this.sessionStorage.get(SESSION_KIND_KEY);
    return value === 'app' || value === 'hosted_sso' ? value : null;
  }

  private clearSession(): void {
    this.sessionStorage.removeMany([TOKEN_KEY, USER_KEY, APPLICATIONS_KEY, SESSION_KIND_KEY]);
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    this.applicationsSignal.set([]);
    this.sessionKindSignal.set(null);
  }
}
