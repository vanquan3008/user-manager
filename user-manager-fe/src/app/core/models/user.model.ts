export interface IdentityUser {
  id: string;
  userName: string;
  email: string;
  emailConfirmed: boolean;
  isActive: boolean;
  isBanned: boolean;
  banReason: string | null;
  createdAt: string;
  userProfileId: string | null;
  /** Có profile domain và đánh dấu SSO (tạo qua SSO / EnableSSO). */
  isSsoAccount?: boolean;
  twoFactorEnabled?: boolean;
  lastLoginAt?: string | null;
  lockoutEnd?: string | null;
}

export interface UserWithRoles {
  user: IdentityUser;
  roles: string[];
}
