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
} from '../models/api-response';
import { generateCodeChallenge, generateCodeVerifier } from '../utils/oauth-pkce';
import { SessionStorageService } from './session-storage.service';

export interface OAuthAuthorizeContext {
  clientId: string;
  redirectUri: string;
  codeVerifier?: string | null;
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
    const params: any = { ...request };
    if (request.scopes) {
      params.scope = request.scopes.join(' ');
      delete params.scopes;
    }
    return this.http.get<ApiResponse<OAuthAuthorizeResponse>>(`${this.oauthApi}/authorize`, { params });
  }


  consent(request: OAuthConsentRequest) {
    return this.http.post<ApiResponse<OAuthAuthorizeResponse>>(`${this.oauthApi}/consent`, request);
  }

  exchangeToken(request: OAuthTokenRequest) {
    return this.http.post<ApiResponse<OAuthTokenResponse>>(`${this.oauthApi}/token`, request);
  }
}
