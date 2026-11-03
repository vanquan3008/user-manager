import { Component, OnInit, inject, signal } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { hasAdminRole } from '../../../core/constants/roles';
import { apiErrorMessage } from '../../../core/utils/http-error.utils';
import type { AuthResponse } from '../../../core/models';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';
import { MfaStepComponent } from '../../../shared/components/mfa-step/mfa-step.component';

@Component({
  selector: 'app-magic-login-callback',
  standalone: true,
  imports: [RouterLink, AuthShellComponent, MfaStepComponent],
  template: `
    <app-auth-shell>
      @if (mfa(); as pending) {
        <app-mfa-step [email]="pending.email" [loading]="loading()" [error]="error()" (verify)="verifyMfa($event)" (back)="router.navigate(['/login'])" />
      } @else if (error()) {
        <h1 class="text-2xl font-semibold tracking-tight">{{ t().authMagicTab }}</h1>
        <div class="alert-bad mt-6" role="alert"><span class="material-symbols-outlined">link_off</span><span>{{ error() }}</span></div>
        <a routerLink="/login" class="btn-secondary btn-block mt-4">{{ t().backToSignIn }}</a>
      } @else {
        <div class="flex items-center gap-3 text-sm text-fg-2" role="status"><span class="spinner text-brand-text"></span>{{ t().magicWorking }}</div>
      }
    </app-auth-shell>
  `
})
export class MagicLoginCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  readonly router = inject(Router);
  private readonly location = inject(Location);
  readonly t = inject(TranslateService).t;

  readonly error = signal('');
  readonly loading = signal(false);
  readonly mfa = signal<{ token: string; email: string } | null>(null);

  ngOnInit(): void {
    // Tokens arrive in the URL fragment (never sent to servers or logs); strip them immediately.
    const fragment = this.route.snapshot.fragment?.trim();
    const params = new URLSearchParams(fragment || '');
    const token = params.get('token') ?? this.route.snapshot.queryParamMap.get('token');
    const purpose = params.get('purpose') ?? 'PasswordlessLogin';
    const clientId = params.get('clientId')?.trim() || undefined;
    this.location.replaceState('/magic-login');

    if (!token) {
      this.error.set(this.t().magicFailed);
      return;
    }

    this.auth.verifyMagicLink(token, purpose, clientId).subscribe({
      next: res => this.complete(res.data),
      error: err => this.error.set(apiErrorMessage(err, this.t().magicFailed))
    });
  }

  verifyMfa(code: string) {
    const pending = this.mfa();
    if (!pending) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.verifyMfa(pending.token, code).subscribe({
      next: res => { this.loading.set(false); this.complete(res.data); },
      error: err => { this.loading.set(false); this.error.set(apiErrorMessage(err, this.t().authFailed)); }
    });
  }

  private complete(data: AuthResponse | null) {
    if (data?.mfaRequired && data.mfaToken) {
      this.mfa.set({ token: data.mfaToken, email: data.user?.email ?? '' });
      return;
    }
    if (!data?.accessToken) {
      this.error.set(this.t().magicFailed);
      return;
    }
    this.auth.setSession(data);
    if (!hasAdminRole(data.user?.roles)) {
      this.auth.logout({ queryParams: { forbidden: '1' } });
      return;
    }
    this.router.navigate(['/'], { replaceUrl: true });
  }
}
