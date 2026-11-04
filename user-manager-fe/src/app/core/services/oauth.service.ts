import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type {
  ApiResponse,
  OAuthAuthorizeRequest,
  OAuthAuthorizeResponse,
  OAuthConsentRequest,
  OAuthTokenRequest,
  OAuthTokenResponse
} from '../models';
import { generateCodeChallenge, generateCodeVerifier } from '../utils/oauth-pkce';
import { SessionStorageService } from './session-storage.service';

export interface OAuthAuthorizeContext {
  clientId: string;
  redirectUri: string;
  codeVerifier?: string | null;
  state?: string | null;
  /** OIDC nonce echoed in the ID token; verified by the callback. */
  nonce?: string | null;
  returnUrl?: string | null;
}

@Injectable({ providedIn: 'root' })
export class OauthService {
  private readonly oauthApi = `${environment.apiUrl}/api/oauth`;
  private readonly authorizeContextStorageKey = 'oauth_authorize_context';

  constructor(
    private http: HttpClient,
    private sessionStorage: SessionStorageService
  ) {}

  async createPkceContext() {
    const codeVerifier = generateCodeVerifier();
    const codeChallenge = await generateCodeChallenge(codeVerifier);
    return { codeVerifier, codeChallenge };
  }

  saveAuthorizeContext(context: OAuthAuthorizeContext) {
    this.sessionStorage.setJson(this.authorizeContextStorageKey, context);
  }

  readAuthorizeContext(): OAuthAuthorizeContext | null {
    return this.sessionStorage.getJson<OAuthAuthorizeContext | null>(this.authorizeContextStorageKey, null);
  }

  clearAuthorizeContext() {
    this.sessionStorage.remove(this.authorizeContextStorageKey);
  }

  authorize(request: OAuthAuthorizeRequest) {
    const params = this.buildAuthorizeParams(request);
    return this.http.get<ApiResponse<OAuthAuthorizeResponse>>(`${this.oauthApi}/authorize`, { params });
  }

  buildHostedAuthorizeUrl(request: OAuthAuthorizeRequest) {
    const params = this.buildAuthorizeParams(request);
    return `/oauth/authorize?${new URLSearchParams(params).toString()}`;
  }

  private buildAuthorizeParams(request: OAuthAuthorizeRequest) {
    const params: Record<string, string> = {
      response_type: 'code',
      client_id: request.clientId,
      redirect_uri: request.redirectUri,
      scope: request.scopes.join(' '),
      code_challenge: request.codeChallenge
    };

    if (request.state?.trim()) {
      params['state'] = request.state.trim();
    }

    if (request.codeChallengeMethod?.trim()) {
      params['code_challenge_method'] = request.codeChallengeMethod.trim();
    }

    if (request.nonce?.trim()) {
      params['nonce'] = request.nonce.trim();
    }

    if (request.prompt?.trim()) {
      params['prompt'] = request.prompt.trim();
    }

    return params;
  }

  consent(request: OAuthConsentRequest) {
    return this.http.post<ApiResponse<OAuthAuthorizeResponse>>(`${this.oauthApi}/consent`, request);
  }

  exchangeToken(request: OAuthTokenRequest) {
    return this.http.post<ApiResponse<OAuthTokenResponse>>(`${this.oauthApi}/token`, request);
  }
}
