import { CommonModule, Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OauthService } from '../../../core/services/oauth.service';
import { getHttpErrorMessage, UNREACHABLE_SERVER_MESSAGE_VI } from '../../../core/utils/http-error.utils';

@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full overflow-y-auto">
      <div class="min-h-full flex items-center justify-center bg-slate-950 bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 px-4 py-8">
        <div class="w-full max-w-md rounded-2xl border border-slate-700/80 bg-slate-900/80 px-6 py-5 shadow-2xl shadow-slate-900/80">
          <h1 class="mb-2 text-lg font-semibold text-slate-50">OAuth callback</h1>
          @if (error()) {
            <p class="rounded-xl border border-rose-500/60 bg-rose-950/50 px-3 py-2 text-sm text-rose-300">
              {{ error() }}
            </p>
          } @else if (token()) {
            <p class="text-sm text-emerald-300">Token exchange thanh cong.</p>
          } @else {
            <p class="text-sm text-slate-300">Dang xu ly callback...</p>
          }
        </div>
      </div>
    </div>
  `
})
export class OauthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly oauth = inject(OauthService);

  readonly error = signal('');
  readonly token = signal('');

  ngOnInit(): void {
    const code = this.route.snapshot.queryParamMap.get('code');
    const context = this.oauth.readAuthorizeContext();

    this.location.replaceState('/oauth/callback');

    if (!code || !context?.clientId || !context.redirectUri || !context.codeVerifier) {
      this.error.set('Thieu OAuth callback context (code/client_id/redirect_uri/code_verifier).');
      return;
    }

    this.oauth
      .exchangeToken({
        grantType: 'authorization_code',
        clientId: context.clientId,
        redirectUri: context.redirectUri,
        code,
        codeVerifier: context.codeVerifier
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data?.accessToken) {
            this.token.set(res.data.accessToken);
            this.oauth.clearAuthorizeContext();
            return;
          }

          this.error.set(res.message || res.error?.message || 'OAuth token exchange that bai.');
        },
        error: (err) => {
          this.error.set(getHttpErrorMessage(err, 'OAuth token exchange that bai.', UNREACHABLE_SERVER_MESSAGE_VI));
        }
      });
  }
}
