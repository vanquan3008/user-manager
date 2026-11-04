export interface OAuthAuthorizeRequest {
  clientId: string;
  redirectUri: string;
  scopes: string[];
  state?: string | null;
  codeChallenge: string;
  codeChallengeMethod?: 'S256' | 'plain';
  prompt?: string | null;
  nonce?: string | null;
}

export interface OAuthAuthorizeResponse {
  requiresConsent: boolean;
  clientName: string;
  redirectUri: string;
  requestedScopes: string[];
  alreadyGrantedScopes: string[];
  missingScopes: string[];
  code?: string | null;
  state?: string | null;
}

export interface OAuthConsentRequest {
  clientId: string;
  redirectUri: string;
  scopes: string[];
  state?: string | null;
  codeChallenge: string;
  codeChallengeMethod?: 'S256' | 'plain';
  approve?: boolean;
  nonce?: string | null;
}

export interface OAuthTokenRequest {
  grantType: 'authorization_code' | 'refresh_token';
  clientId: string;
  redirectUri: string;
  code?: string | null;
  codeVerifier?: string | null;
  refreshToken?: string | null;
}

export interface OAuthTokenResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  refreshToken: string;
  scopes: string[];
  idToken?: string | null;
  user: {
    id: string;
    email: string;
    username: string;
    displayName?: string;
    roles: string[];
  };
}
