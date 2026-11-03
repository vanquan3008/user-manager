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
  /** True when the password was correct but a second factor is required. */
  mfaRequired?: boolean;
  mfaToken?: string | null;
  mfaExpiresAt?: string | null;
  sessionId?: string | null;
}

export interface HostedSsoLoginResponse {
  accessToken: string;
  refreshToken?: string | null;
  user: {
    userId: string;
    email: string;
    username: string;
    displayName?: string;
    roles: string[];
    permissions?: string[];
  };
  mfaRequired?: boolean;
  mfaToken?: string | null;
  mfaExpiresAt?: string | null;
  sessionId?: string | null;
}

export interface SsoUserApplication {
  id: string;
  name: string;
  role: string;
}

export interface SsoExchangeTokenRequest {
  targetApplicationId: string;
}
