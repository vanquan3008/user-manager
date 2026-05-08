export interface SubscriptionDto {
  id: string;
  userId: string;
  applicationId: string;
  licensePackageId: string;
  subscriptionType: 'Free' | 'Premium' | 'Family' | 'Student';
  startedAt: string;
  expiresAt: string;
  cancelledAt: string | null;
  isActive: boolean;
  maxMembers: number;
  currentMemberCount: number;
  entitlement: EntitlementSnapshotDto;
  createdAt: string;
  updatedAt: string | null;
  members?: SubscriptionMemberDto[];
}

export interface SubscriptionMemberDto {
  id: string;
  subscriptionId: string;
  userId: string;
  userEmail: string;
  userName: string;
  roleType: string;
  isAdmin: boolean;
  isOwner: boolean;
  isActive: boolean;
  joinedAt: string;
  lastActivityAt?: string;
}

export interface EntitlementSnapshotDto {
  features: string[];
  quotas: {
    maxUsers: number;
    maxApiCalls: number | null;
    maxMembers: number;
  };
  assignedRole: string;
  planMetadata?: {
    planId: string;
    packageName: string;
  };
}

export interface SubscriptionPlanCatalogDto {
  id: string;
  applicationId: string | null;
  name: string;
  description: string;
  monthlyPrice: number;
  yearlyPrice: number;
  maxUsers: number;
  maxApiCalls: number | null;
  includeAnalytics: boolean;
  includeSupport: boolean;
  includeAdvancedSecurity: boolean;
  includeCustomDomain: boolean;
  isActive: boolean;
  displayOrder: number;
  assignedRoleIds: string[];
  assignedRole: string;
  icon: string | null;
}

export interface LicensePackageCatalogDto {
  id: string;
  applicationId: string;
  subscriptionPlanId: string | null;
  name: string;
  description: string | null;
  features: string[];
  seatLimit: number;
  isActive: boolean;
  displayOrder: number;
  assignedRole: string;
}
