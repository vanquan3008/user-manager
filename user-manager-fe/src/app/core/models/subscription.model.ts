export type SubscriptionType = 'Free' | 'Premium' | 'Family' | 'Student';

export interface SubscriptionDto {
  id: string;
  userId: string;
  applicationId: string;
  subscriptionPlanId: string | null;
  subscriptionType: SubscriptionType;
  startedAt: string;
  expiresAt: string;
  cancelledAt: string | null;
  isActive: boolean;
  maxMembers: number;
  currentMemberCount: number;
  entitlementSnapshot: EntitlementSnapshotDto | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface EntitlementSnapshotDto {
  features: string[];
  quotas: {
    maxUsers: number;
    maxApiCalls: number | null;
    maxMembers: number;
  };
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
  features: string[];
  permissions: string[];
  isActive: boolean;
  displayOrder: number;
  icon: string | null;
}
