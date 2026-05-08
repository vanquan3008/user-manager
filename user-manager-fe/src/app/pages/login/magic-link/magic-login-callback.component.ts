import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { hasAdminRole } from '../../../core/constants/roles';
import type { AuthResponse } from '../../../core/models/api-response';

@Component({
  selector: 'app-magic-login-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full overflow-y-auto">
      <div class="min-h-full flex items-center justify-center bg-slate-950 bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 px-4 py-8">
        <div class="w-full max-w-md rounded-2xl border border-slate-700/80 bg-slate-900/80 px-6 py-5 shadow-2xl shadow-slate-900/80">
          <h1 class="mb-2 text-lg font-semibold text-slate-50">Authenticating...</h1>
          @if (error) {
            <p class="rounded-xl border border-rose-500/60 bg-rose-950/50 px-3 py-2 text-sm text-rose-300">
              {{ error }}
            </p>
          } @else {
            <p class="text-sm text-slate-300">
              Please wait while we verify your sign-in link.
            </p>
          }
        </div>
      </div>
    </div>
  `
})
export class MagicLoginCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly location = inject(Location);

  error = '';

  ngOnInit(): void {
    const params = this.readLinkParams();
    const token = params.get('token');
    const purpose = params.get('purpose') ?? 'PasswordlessLogin';
    const clientId = params.get('clientId')?.trim() || undefined;

    this.clearSensitiveUrlState();

    if (!token) {
      this.error = 'Unable to validate this sign-in link.';
      return;
    }

    this.auth.verifyMagicLink(token, purpose, clientId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const auth = res.data as AuthResponse;
          if (!hasAdminRole(auth?.user?.roles)) {
            this.error = 'Only Admin or SuperAdmin accounts can sign in here.';
            return;
          }

          this.auth.setSession(auth);
          this.router.navigate(['/'], { replaceUrl: true });
          return;
        }

        this.error = res.message || res.error?.message || 'The sign-in link is invalid or expired.';
      },
      error: (err) => {
        this.error = err.error?.message || err.error?.error?.message || 'The sign-in link is invalid or expired.';
      }
    });
  }

  private readLinkParams(): URLSearchParams {
    const fragment = this.route.snapshot.fragment?.trim();
    if (fragment) {
      return new URLSearchParams(fragment);
    }

    const params = new URLSearchParams();
    const queryToken = this.route.snapshot.queryParamMap.get('token');
    const queryPurpose = this.route.snapshot.queryParamMap.get('purpose');
    const queryClientId = this.route.snapshot.queryParamMap.get('clientId');
    if (queryToken) params.set('token', queryToken);
    if (queryPurpose) params.set('purpose', queryPurpose);
    if (queryClientId) params.set('clientId', queryClientId);
    return params;
  }

  private clearSensitiveUrlState(): void {
    this.location.replaceState('/magic-login');
  }
}
