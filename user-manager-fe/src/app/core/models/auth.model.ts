export interface SsoApplication {
  id: string;
  name: string;
  description?: string;
  role: string;
  canRead: boolean;
  canWrite: boolean;
  canDelete: boolean;
  canManageUsers: boolean;
  canManageSubscription: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken?: string | null;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    username: string;
    displayName?: string;
    roles: string[];
    isSsoAccount?: boolean;
  };
  applications?: SsoApplication[];
}

export interface SsoUserApplication {
  id: string;
  name: string;
  role: string;
}
