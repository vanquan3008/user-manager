export interface SsoApplication {
  id: string;
  name: string;
  description?: string;
  role?: string;
  canRead?: boolean;
  canWrite?: boolean;
  canDelete?: boolean;
  canManageUsers?: boolean;
  canManageSubscription?: boolean;
}

export type AuthenticatedUser = {
  id: string;
  email: string;
  username: string;
  displayName?: string;
  roles: string[];
  permissions?: string[];
};

export interface AuthResponse {
  accessToken: string;
  refreshToken?: string | null;
  expiresAt: string;
  user: AuthenticatedUser & {
    isSsoAccount?: boolean;
  };
  applications?: SsoApplication[];
}

export interface SsoUserApplication {
  id: string;
  name: string;
  role: string;
}

export interface SsoAuthorizeRequest {
  email: string;
  password: string;
  clientId?: string;
  redirectUri: string;
  scopes: string[];
  state?: string;
  codeChallenge: string;
  codeChallengeMethod?: string;
}

export interface SsoConsentRequest extends SsoAuthorizeRequest {
  approve: boolean;
}

export interface SsoAuthorizeResponse {
  requiresConsent: boolean;
  code?: string;
  redirectUri: string;
  state?: string;
  clientName?: string;
  requestedScopes?: string[];
  missingScopes?: string[];
}

export interface SsoExchangeTokenRequest {
  targetApplicationId: string;
}
