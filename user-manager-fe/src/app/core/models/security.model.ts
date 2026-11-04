export interface SessionInfo {
  id: string;
  kind: 'console' | 'hosted_sso' | string;
  authenticationMethod: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt: string;
  lastSeenAt: string;
  expiresAt: string;
  isCurrent: boolean;
}

export interface MfaStatus {
  enabled: boolean;
  enabledAt?: string | null;
  recoveryCodesRemaining: number;
}

export interface MfaSetup {
  sharedKey: string;
  authenticatorUri: string;
}

export interface AuditLogEntry {
  id: string;
  occurredAt: string;
  eventType: string;
  category: string;
  succeeded: boolean;
  actorUserId?: string | null;
  actorEmail?: string | null;
  targetUserId?: string | null;
  targetType?: string | null;
  targetId?: string | null;
  applicationId?: string | null;
  sessionId?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
  correlationId?: string | null;
  reason?: string | null;
  details?: string | null;
}

export interface AuditLogFilter {
  page: number;
  pageSize: number;
  search?: string;
  category?: string;
  succeeded?: boolean | null;
  from?: string;
  to?: string;
  userId?: string;
}

export interface UserStatistics {
  total: number;
  active: number;
  banned: number;
  locked: number;
  mfaEnabled: number;
  emailVerified: number;
  newSince: number;
  signedInSince: number;
}

export interface DailyLoginPoint {
  day: string;
  succeeded: number;
  failed: number;
}

export interface DashboardSummary {
  users: UserStatistics;
  applications: number;
  activeSessions: number;
  mfaAdoptionRate: number;
  loginActivity: DailyLoginPoint[];
  recentSecurityEvents: AuditLogEntry[];
  generatedAt: string;
}

export interface SigningKeyInfo {
  keyId: string;
  algorithm: string;
  keySize: number;
  status: 'active' | 'retired' | 'revoked';
  createdAt: string;
  retiredAt?: string | null;
  revokedAt?: string | null;
  publishedUntil?: string | null;
}

export interface SecuritySettings {
  issuer: string;
  audience: string;
  accessTokenMinutes: number;
  refreshTokenIdleDays: number;
  sessionAbsoluteDays: number;
  refreshReuseGraceSeconds: number;
  mfaChallengeMinutes: number;
  signingKeyRotationDays: number;
  retiredKeyRetentionDays: number;
  auditRetentionDays: number;
  rateLimitRequests: number;
  rateLimitWindowSeconds: number;
  authRateLimitRequests: number;
  authRateLimitWindowSeconds: number;
  passwordMinLength: number;
  passwordRequiresDigit: boolean;
  passwordRequiresUppercase: boolean;
  passwordRequiresLowercase: boolean;
  passwordRequiresSymbol: boolean;
  lockoutMaxFailedAttempts: number;
  lockoutMinutes: number;
  discoveryUrl: string;
  jwksUrl: string;
}
