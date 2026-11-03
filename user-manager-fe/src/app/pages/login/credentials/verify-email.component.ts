import { Component, OnInit, inject, signal } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateService } from '../../../core/services/translate.service';
import { apiErrorMessage } from '../../../core/utils/http-error.utils';
import { AuthShellComponent } from '../../../shared/components/auth-shell/auth-shell.component';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [RouterLink, AuthShellComponent],
  template: `
    <app-auth-shell>
      <h1 class="text-2xl font-semibold tracking-tight">{{ t().verifyTitle }}</h1>
      <div class="mt-6">
        @if (loading()) {
          <div class="flex items-center gap-3 text-sm text-fg-2" role="status"><span class="spinner text-brand-text"></span>{{ t().verifyWorking }}</div>
        } @else if (success()) {
          <div class="alert-ok" role="status"><span class="material-symbols-outlined">verified</span><span>{{ t().verifyDone }}</span></div>
        } @else {
          <div class="alert-bad" role="alert"><span class="material-symbols-outlined">error</span><span>{{ error() || t().verifyFailed }}</span></div>
        }
      </div>
      @if (!loading()) { <a routerLink="/login" class="btn-secondary btn-block mt-6">{{ t().backToSignIn }}</a> }
    </app-auth-shell>
  `
})
export class VerifyEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly location = inject(Location);
  readonly t = inject(TranslateService).t;
  readonly error = signal('');
  readonly loading = signal(true);
  readonly success = signal(false);

  ngOnInit() {
    const params = this.readLinkParams();
    const token = params.get('token');

    this.clearSensitiveUrlState();

    if (!token) {
      this.error.set(this.t().verifyFailed);
      this.loading.set(false);
      return;
    }

    this.auth.verifyEmail(token).subscribe({
      next: (res: any) => {
        this.success.set(res.success !== false);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err, this.t().verifyFailed));
        this.loading.set(false);
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
    if (queryToken) params.set('token', queryToken);
    return params;
  }

  private clearSensitiveUrlState(): void {
    this.location.replaceState('/verify-email');
  }
}
