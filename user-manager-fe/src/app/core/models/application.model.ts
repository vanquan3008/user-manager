export interface ApplicationListItem {
  id: string;
  name: string;
  clientId?: string;
  clientType?: string;
  clientSecret?: string | null;
  lastClientSecretRotatedAt?: string | null;
  description?: string;
  ownerId?: string;
  ownerDisplayName?: string | null;
  logoUrl?: string;
  backchannelLogoutUri?: string;
  allowedRedirectUris?: string;
}

export interface ApplicationCredentialItem {
  applicationId: string;
  applicationName: string;
  clientId: string;
  clientType: string;
  clientSecret?: string | null;
  lastClientSecretRotatedAt?: string | null;
  allowedRedirectUris?: string;
  backchannelLogoutUri?: string;
}

export interface ApplicationUserDto {
  userId: string;
  role: string;
  email?: string;
  userName?: string;
}

export interface ApplicationCapabilitiesDto {
  applicationId: string;
  features: Record<string, boolean>;
  permissions: Record<string, boolean>;
  enabledFeatures: string[];
  enabledPermissions: string[];
}
