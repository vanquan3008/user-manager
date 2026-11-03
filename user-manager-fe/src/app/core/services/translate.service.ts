import { Injectable, signal, computed } from '@angular/core';

export type Locale = 'vi' | 'en' | 'zh';

export interface AppTranslations {
  // Dashboard
  overview: string;
  overviewDesc: string;
  totalUsers: string;
  totalUsersDesc: string;
  activeUsers: string;
  activeUsersDesc: string;
  roles: string;
  rolesDesc: string;
  subscriptions: string;
  subscriptionsDesc: string;
  userDistribution: string;
  total: string;
  active: string;
  blocked: string;
  loginChartTitle: string;
  loginCount: string;
  trend: string;
  noData: string;
  quickAccess: string;
  /** Biểu đồ nhỏ dưới Truy cập nhanh */
  quickAccessChartTitle: string;
  inactive: string;
  /** Khối thông tin app dưới biểu đồ (dashboard) */
  dashboardAppInfoTitle: string;
  dashboardAppInfoEmpty: string;
  userBan: string;
  userUnban: string;
  banUserConfirm: string;
  viewAll: string;
  recentUsers: string;
  noUsers: string;
  loading: string;
  badge: string;
  // Nav
  navOverview: string;
  navUsers: string;
  navApplications: string;
  navRoles: string;
  navSubscriptions: string;
  navPermissions: string;
  navRolePermission: string;
  logout: string;
  searchPlaceholder: string;
  // Common
  close: string;
  /** {{from}}, {{to}}, {{total}} */
  paginationShowing: string;
  paginationPrev: string;
  paginationNext: string;
  paginationPerPage: string;
  cancel: string;
  save: string;
  add: string;
  edit: string;
  select: string;
  description: string;
  type: string;
  startDate: string;
  expiryDate: string;
  members: string;
  noOptions: string;
  selectUser: string;
  selectApplication: string;
  // Subscriptions
  subsPageDesc: string;
  subsSelectUserApp: string;
  subsLoadSubscription: string;
  subsActiveSubscription: string;
  subsNoActiveSubscription: string;
  subsActivate: string;
  subsCancelSubscription: string;
  subsHistory: string;
  subsNoHistory: string;
  subsActivateModalTitle: string;
  subsPlanId: string;
  subsPlanType: string;
  subsProcessing: string;
  subsLoadingUsers: string;
  subsAddMember: string;
  subsRemoveMember: string;
  subsMemberLimit: string;
  subsJoinedAt: string;
  subscriptionCatalogDesc: string;

  packageCatalogDesc: string;
  changePasswordDesc: string;
  // Permissions
  permissionsTitle: string;
  permissionsDesc: string;
  permissionsAdd: string;
  permissionsSearchPlaceholder: string;
  permissionsAllCategories: string;
  /** Khóa chữ thường (theo API/code) → nhãn hiển thị trong filter danh mục */
  permissionCategoryLabels: Record<string, string>;
  permissionsLoading: string;
  permissionsCode: string;
  permissionsCategory: string;
  permissionsSystem: string;
  permissionsScope: string;
  permissionsYes: string;
  permissionsNo: string;
  permissionsEmpty: string;
  permissionsAddNew: string;
  permissionsAddModalTitle: string;
  permissionsResource: string;
  permissionsAction: string;
  permissionsViewUsers: string;
  permissionsCreateNew: string;
  permissionsCodePlaceholder: string;
  permissionsCategoryPlaceholder: string;
  permissionsScopePlaceholder: string;
  permissionsDescriptionPlaceholder: string;
  // Roles
  rolesTitle: string;
  rolesPageDesc: string;
  rolesAssignPermissions: string;
  rolesAddRole: string;
  rolesSearchPlaceholder: string;
  rolesName: string;
  rolesSystem: string;
  rolesActions: string;
  rolesEdit: string;
  rolesDelete: string;
  rolesDeleting: string;
  rolesSaving: string;
  rolesEmpty: string;
  rolesAddRoleHint: string;
  rolesAddModalTitle: string;
  rolesEditModalTitle: string;
  rolesAddNew: string;
  // Users list
  usersPageDesc: string;
  // Applications
  appsPageDesc: string;
  appsAdd: string;
  appsEmpty: string;
  appsAddHint: string;
  appsAddModalTitle: string;
  appsDescription: string;
  appsDescriptionPlaceholder: string;
  // Role-Permission
  rolePermDesc: string;
  rolePermManageLink: string;
  rolePermSelectRole: string;
  rolePermSelectRoleHint: string;
  rolePermNoRoles: string;
  rolePermNoPermissions: string;
  rolePermCreateNew: string;
  rolePermLoadingRolePerms: string;
  // User-admin
  userAdminDesc: string;
  userAdminQuickView: string;
  userAdminSelectPlan: string;
  userAdminAddRoleModal: string;
  // Login
  loginNoAccount: string;
  loginRegister: string;
  loginNoAccountRegister: string;
  // OAuth hosted consent
  oauthAuthorizeEyebrow: string;
  oauthAuthorizeTitle: string;
  oauthAuthorizeSubtitle: string;
  oauthAuthorizeAppLabel: string;
  oauthAuthorizeRedirectLabel: string;
  oauthAuthorizeRequestedScopes: string;
  oauthAuthorizeAlreadyGranted: string;
  oauthAuthorizeNewAccess: string;
  oauthAuthorizeLoading: string;
  oauthAuthorizeRedirecting: string;
  oauthAuthorizeReviewNote: string;
  oauthAuthorizeApprove: string;
  oauthAuthorizeDeny: string;
  oauthAuthorizeMissingParams: string;
  oauthAuthorizeFailed: string;
  oauthAuthorizeConsentFailed: string;
  oauthAuthorizeGrantedBadge: string;
  oauthAuthorizeRequestedBadge: string;
  // OAuth callback
  oauthCallbackTitle: string;
  oauthCallbackSubtitle: string;
  oauthCallbackLoading: string;
  oauthCallbackSuccess: string;
  oauthCallbackFailed: string;
  oauthCallbackMissingContext: string;
  oauthCallbackStateMismatch: string;
  oauthCallbackAdminOnly: string;
  oauthCallbackLoginFailed: string;
  // Layout chrome & page header
  layoutBrandEyebrow: string;
  layoutBrandTitle: string;
  navGroupPrimary: string;
  navGroupAdmin: string;
  navCatalog: string;
  navSecurity: string;
  headerWorkspaceSuffix: string;
  layoutDrawerWorkspaceAction: string;
  ariaCloseNavigation: string;
  ariaOpenNavigation: string;
  ariaSwitchToDark: string;
  ariaSwitchToLight: string;
  layoutPageDashboard: string;
  layoutEyebrowOverview: string;
  layoutPageUsers: string;
  layoutEyebrowIdentity: string;
  layoutEyebrowUserDetail: string;
  layoutPageApplications: string;
  layoutEyebrowAppList: string;
  layoutEyebrowAppDetail: string;
  layoutPageRoles: string;
  layoutEyebrowAuthModel: string;
  layoutPageSubscriptions: string;
  layoutEyebrowCommerce: string;
  layoutPageSubscriptionCatalog: string;
  layoutEyebrowPlanCatalogShort: string;
  layoutPagePackageCatalog: string;
  layoutEyebrowPackages: string;
  layoutPagePermissions: string;
  layoutEyebrowAccessControl: string;
  layoutPageRoleMapping: string;
  layoutEyebrowPermAssignment: string;
  layoutPageSecuritySettings: string;
  layoutEyebrowAccountSettings: string;
  layoutPageUserAdmin: string;
  layoutEyebrowCrmWorkspace: string;
  layoutPageAdmin: string;
  layoutEyebrowWorkspace: string;
  // Dashboard
  dashLoading: string;
  dashRetry: string;
  dashCardActiveSuffix: string;
  dashCardSsoSuffix: string;
  dashCardAppsSub: string;
  dashSsoAccountsTitle: string;
  dashRegTrend: string;
  dashRegTrendDesc: string;
  dashDaysLabel: string;
  dashAccountHealth: string;
  dashAccountHealthDesc: string;
  dashRolesWord: string;
  dashActiveAccounts: string;
  dashEmailVerified: string;
  dashBannedAccounts: string;
  dashSsoCoverage: string;
  dashAppCatalogTitle: string;
  dashAppCatalogDesc: string;
  dashViewAll: string;
  dashNoDescription: string;
  dashRecentUsersTitle: string;
  dashRecentUsersDesc: string;
  dashManageUsers: string;
  dashEmptyApps: string;
  dashEmptyUsers: string;
  dashBannedBadge: string;
  // --- console redesign
  navGroupOverview: string;
  navGroupIdentity: string;
  navGroupAccess: string;
  navGroupSecurity: string;
  navGroupBilling: string;
  navDashboard: string;
  navSsoAccounts: string;
  navAuditLogs: string;
  navSystem: string;
  navAccount: string;
  signOut: string;
  signOutEverywhere: string;
  toggleTheme: string;
  themeLight: string;
  themeDark: string;
  language: string;
  productName: string;
  productTagline: string;
  skipToContent: string;
  openMenu: string;
  accountMenu: string;
  authConsoleTitle: string;
  authConsoleSubtitle: string;
  authEmail: string;
  authPassword: string;
  authShowPassword: string;
  authHidePassword: string;
  authForgot: string;
  authSignIn: string;
  authSigningIn: string;
  authPasswordTab: string;
  authMagicTab: string;
  authMagicIntro: string;
  authMagicSend: string;
  authMagicSending: string;
  authMagicSent: string;
  authOr: string;
  authSsoButton: string;
  authSsoMissingConfig: string;
  authInvalidEmail: string;
  authPasswordRequired: string;
  authAdminOnly: string;
  authSessionExpired: string;
  authSignedIn: string;
  authFailed: string;
  authSecureFooter: string;
  authHeroTitle: string;
  authHeroBody: string;
  authHeroPoint1: string;
  authHeroPoint2: string;
  authHeroPoint3: string;
  mfaTitle: string;
  mfaSubtitle: string;
  mfaRecoverySubtitle: string;
  mfaCode: string;
  mfaRecoveryCode: string;
  mfaVerify: string;
  mfaVerifying: string;
  mfaUseRecovery: string;
  mfaUseAuthenticator: string;
  mfaBack: string;
  mfaSigningInAs: string;
  mfaExpired: string;
  ssoPortalTitle: string;
  ssoPortalSubtitle: string;
  ssoContinuingTo: string;
  registerLink: string;
  registerTitle: string;
  registerSubtitle: string;
  registerFirstName: string;
  registerLastName: string;
  registerUsername: string;
  registerDob: string;
  registerSubmit: string;
  registerSubmitting: string;
  registerDone: string;
  registerHaveAccount: string;
  passwordPolicyHint: string;
  forgotTitle: string;
  forgotSubtitle: string;
  forgotSubmit: string;
  forgotSent: string;
  resetTitle: string;
  resetSubtitle: string;
  resetNewPassword: string;
  resetConfirmPassword: string;
  resetSubmit: string;
  resetDone: string;
  resetMismatch: string;
  resetInvalidLink: string;
  verifyTitle: string;
  verifyWorking: string;
  verifyDone: string;
  verifyFailed: string;
  magicWorking: string;
  magicFailed: string;
  backToSignIn: string;
  dashTitle: string;
  dashSubtitle: string;
  dashRefresh: string;
  dashKpiUsers: string;
  dashKpiUsersSub: string;
  dashKpiSessions: string;
  dashKpiSessionsSub: string;
  dashKpiMfa: string;
  dashKpiMfaSub: string;
  dashKpiApps: string;
  dashKpiAppsSub: string;
  dashLoginActivity: string;
  dashLoginActivityDesc: string;
  dashSucceeded: string;
  dashFailed: string;
  dashChart: string;
  dashTable: string;
  dashIdentityHealth: string;
  dashIdentityHealthDesc: string;
  dashActive: string;
  dashVerified: string;
  dashLocked: string;
  dashBanned: string;
  dashSignedIn30: string;
  dashRecentEvents: string;
  dashViewAuditLog: string;
  dashNoEvents: string;
  dashRecentUsers: string;
  dashLoadFailed: string;
  dashMfaNudge: string;
  statusSucceeded: string;
  statusFailed: string;
  accTitle: string;
  accSubtitle: string;
  accTabProfile: string;
  accTabSecurity: string;
  accTabSessions: string;
  accTabActivity: string;
  accUsername: string;
  accEmail: string;
  accRoles: string;
  accMemberSince: string;
  accEmailStatus: string;
  accVerified: string;
  accUnverified: string;
  accAccountType: string;
  accSso: string;
  accConsole: string;
  accPasswordTitle: string;
  accPasswordDesc: string;
  accCurrentPassword: string;
  accChangePassword: string;
  accPasswordChanged: string;
  accMfaTitle: string;
  accMfaDesc: string;
  accMfaOn: string;
  accMfaOff: string;
  accMfaEnable: string;
  accMfaScan: string;
  accMfaManual: string;
  accMfaEnterCode: string;
  accMfaConfirm: string;
  accMfaEnabled: string;
  accMfaDisable: string;
  accMfaDisableDesc: string;
  accMfaDisabled: string;
  accRecoveryTitle: string;
  accRecoveryDesc: string;
  accRecoveryRemaining: string;
  accRecoveryRegenerate: string;
  accRecoveryCopy: string;
  accRecoveryDownload: string;
  accRecoveryDone: string;
  accCopied: string;
  accCodeFromApp: string;
  accSessionsDesc: string;
  accSignOutOthers: string;
  accSignOutOthersConfirm: string;
  accThisDevice: string;
  accRevoke: string;
  accRevokeConfirm: string;
  accLastActive: string;
  accSignedIn: string;
  accExpires: string;
  accKindConsole: string;
  accKindSso: string;
  accNoSessions: string;
  accActivityDesc: string;
  accNoActivity: string;
  colEvent: string;
  colResult: string;
  colIp: string;
  colDevice: string;
  colWhen: string;
  colActor: string;
  colTarget: string;
  auditTitle: string;
  auditSubtitle: string;
  auditExport: string;
  auditSearch: string;
  auditAllCategories: string;
  auditAllResults: string;
  auditOnlySucceeded: string;
  auditOnlyFailed: string;
  auditFrom: string;
  auditTo: string;
  auditClear: string;
  auditEmpty: string;
  auditDetail: string;
  auditCatAuth: string;
  auditCatSession: string;
  auditCatMfa: string;
  auditCatOauth: string;
  auditCatUser: string;
  auditCatApplication: string;
  auditCatSecurity: string;
  auditField: string;
  auditReason: string;
  auditCorrelation: string;
  auditSession: string;
  auditApplication: string;
  auditUserAgent: string;
  auditDetailsJson: string;
  sysTitle: string;
  sysSubtitle: string;
  sysKeysTitle: string;
  sysKeysDesc: string;
  sysRotate: string;
  sysRotateConfirm: string;
  sysRotated: string;
  sysRevoke: string;
  sysRevokeConfirm: string;
  sysRevoked: string;
  sysKeyId: string;
  sysAlgorithm: string;
  sysStatus: string;
  sysCreated: string;
  sysPublishedUntil: string;
  sysActive: string;
  sysRetired: string;
  sysRevokedStatus: string;
  sysEndpoints: string;
  sysEndpointsDesc: string;
  sysIssuer: string;
  sysAudience: string;
  sysDiscovery: string;
  sysJwks: string;
  sysCopy: string;
  sysPolicy: string;
  sysPolicyDesc: string;
  sysTokens: string;
  sysAccessToken: string;
  sysRefreshIdle: string;
  sysSessionAbsolute: string;
  sysReuseGrace: string;
  sysMfaChallenge: string;
  sysKeyRotation: string;
  sysKeyRetention: string;
  sysProtection: string;
  sysPasswordPolicy: string;
  sysLockout: string;
  sysRateLimit: string;
  sysAuthRateLimit: string;
  sysAuditRetention: string;
  sysMinutes: string;
  sysDays: string;
  sysSeconds: string;
  sysPerWindow: string;
  sysLockoutValue: string;
  sysDisabled: string;
  sysNoPermission: string;
  usersTitle: string;
  usersSubtitle: string;
  usersSearch: string;
  usersNewSso: string;
  usersColUser: string;
  usersColStatus: string;
  usersColSecurity: string;
  usersColType: string;
  usersColLastSignIn: string;
  usersColJoined: string;
  usersNever: string;
  usersBanned: string;
  usersInactive: string;
  usersActive: string;
  usersLocked: string;
  usersUnverified: string;
  usersNoMfa: string;
  usersEmpty: string;
  usersStandard: string;
  usersSso: string;
  usersCreateSsoInfo: string;
  usersRequired: string;
  usersCreated: string;
  creating: string;
  create: string;
  udBack: string;
  udCopyEmail: string;
  udCopyId: string;
  udDisable: string;
  udEnable: string;
  udRoles: string;
  udRolesDesc: string;
  udAddRole: string;
  udSelectRole: string;
  udNoRoles: string;
  udRemoveRole: string;
  udApps: string;
  udAppsDesc: string;
  udGrant: string;
  udSelectApp: string;
  udSelectAppRole: string;
  udNoApps: string;
  udNoAppsAvailable: string;
  udRevoke: string;
  udSecurity: string;
  udSecurityDesc: string;
  udMfaReset: string;
  udMfaResetConfirm: string;
  udMfaResetDone: string;
  udRevokeAll: string;
  udRevokeAllConfirm: string;
  udActiveSessions: string;
  udRecentActivity: string;
  udBanConfirm: string;
  udUserId: string;
  udBanReason: string;
  ssoAccountsTitle: string;
  ssoAccountsSubtitle: string;
  appsTitle: string;
  appsSubtitle: string;
  appsNew: string;
  appsSearch: string;
  appsSortName: string;
  appsSortOwner: string;
  appsSortBranding: string;
  appsGrid: string;
  appsList: string;
  appsEmptyDesc: string;
  appsOpen: string;
  appsDelete: string;
  appsDeleteConfirm: string;
  appsDeleted: string;
  appsColApp: string;
  appsColClientId: string;
  appsColType: string;
  appsColOwner: string;
  appsName: string;
  appsClientType: string;
  appsPublic: string;
  appsPublicDesc: string;
  appsConfidential: string;
  appsConfidentialDesc: string;
  appsRedirects: string;
  appsRedirectsHint: string;
  appsBackchannel: string;
  appsBackchannelHint: string;
  appsLogo: string;
  appsUploadLogo: string;
  appsLogoHint: string;
  appsNameRequired: string;
  appsCreated: string;
  appsCredentials: string;
  appsSecretOnce: string;
  appsClientSecret: string;
  appsDone: string;
  copied: string;
  adBack: string;
  adChangeLogo: string;
  adOverview: string;
  adOverviewDesc: string;
  adOwner: string;
  adNoRedirects: string;
  adNotConfigured: string;
  adCredentials: string;
  adPublicNote: string;
  adRotate: string;
  adLastRotated: string;
  adNeverRotated: string;
  adShow: string;
  adHide: string;
  adIntegration: string;
  adIntegrationDesc: string;
  adAuthorizeEndpoint: string;
  adTokenEndpoint: string;
  adMembers: string;
  adMembersDesc: string;
  adAddMember: string;
  adRemoveMember: string;
  adRemoveMemberConfirm: string;
  adMemberRemoved: string;
  adNoMembers: string;
  adNeedRole: string;
  adUser: string;
  adRole: string;
  adRoles: string;
  adRolesDesc: string;
  adAddRole: string;
  adGlobalRole: string;
  adNoRoles: string;
  adCapabilities: string;
  adCapabilitiesDesc: string;
  adFeatures: string;
  adPermissions: string;
  adNone: string;
  adSelectUser: string;
  adSelectRole: string;
  adding: string;
  rlTitle: string;
  rlSubtitle: string;
  rlNew: string;
  rlEdit: string;
  rlSearch: string;
  rlColRole: string;
  rlColPermissions: string;
  rlColUpdated: string;
  rlSystem: string;
  rlMore: string;
  rlEmpty: string;
  rlName: string;
  rlDescription: string;
  rlPermissionsPick: string;
  rlPermissionsSearch: string;
  rlSystemLocked: string;
  rlDeleteConfirm: string;
  rlNameRequired: string;
  rlSaved: string;
  rlDeleted: string;
  saving: string;
  delete: string;
  pmTitle: string;
  pmSubtitle: string;
  pmNew: string;
  pmSearch: string;
  pmColCode: string;
  pmColCategory: string;
  pmColDescription: string;
  pmColScope: string;
  pmSystem: string;
  pmEmpty: string;
  pmCodeHint: string;
  pmScopeHint: string;
  pmCreated: string;
  rpTitle: string;
  rpSubtitle: string;
  rpSearchRoles: string;
  rpSelectRole: string;
  rpAssign: string;
  rpPickPermission: string;
  rpNoPermissions: string;
  rpAssigned: string;
  rpRemoved: string;
  rpRemove: string;
  sbTitle: string;
  sbSubtitle: string;
  sbApplication: string;
  sbMember: string;
  sbSelectApp: string;
  sbSelectMember: string;
  sbPickPrompt: string;
  sbNoMembers: string;
  sbActive: string;
  sbNone: string;
  sbActivate: string;
  sbCancel: string;
  sbCancelConfirm: string;
  sbCancelled: string;
  sbActivated: string;
  sbType: string;
  sbStarted: string;
  sbExpires: string;
  sbMembers: string;
  sbApiCalls: string;
  sbFeatures: string;
  sbHistory: string;
  sbNoHistory: string;
  sbStatus: string;
  sbStatusActive: string;
  sbStatusCancelled: string;
  sbStatusExpired: string;
  sbPlan: string;
  sbChoosePlan: string;
  sbNoPlans: string;
  sbPickPlan: string;
  sbPerMonth: string;
  sbUsers: string;
  ctTitle: string;
  ctSubtitle: string;
  ctNew: string;
  ctEdit: string;
  ctPickApp: string;
  ctSearch: string;
  ctEmpty: string;
  ctMonthly: string;
  ctYearly: string;
  ctMaxUsers: string;
  ctMaxApiCalls: string;
  ctUnlimitedHint: string;
  ctFeatures: string;
  ctOrder: string;
  ctIcon: string;
  ctActive: string;
  ctDraft: string;
  ctLive: string;
  ctSave: string;
  ctDeleteConfirm: string;
  ctSaved: string;
  ctDeleted: string;
  ctNameRequired: string;
  ctSave2: string;
  ctFeatAnalytics: string;
  ctFeatSupport: string;
  ctFeatSecurity: string;
  ctFeatDomain: string;
  ctPerYear: string;
}

const TRANSLATIONS: { en: AppTranslations } & Record<Exclude<Locale, 'en'>, Partial<AppTranslations>> = {
  vi: {
    overview: 'Tổng quan',
    overviewDesc: 'Quản lý người dùng, phân quyền và gói dịch vụ cho đội ngũ truyền thông xã hội.',
    totalUsers: 'Tổng người dùng',
    totalUsersDesc: 'Xem danh sách người dùng',
    activeUsers: 'Đang hoạt động',
    activeUsersDesc: 'Active & không bị chặn',
    roles: 'Roles',
    rolesDesc: 'Quản lý phân quyền',
    subscriptions: 'Subscriptions',
    subscriptionsDesc: 'Gói dịch vụ & nâng cấp',
    userDistribution: 'Phân bố users',
    total: 'Tổng',
    active: 'Active',
    blocked: 'Bị chặn',
    loginChartTitle: 'User đăng nhập (7 ngày gần nhất)',
    loginCount: 'Lượt đăng nhập',
    trend: 'Xu hướng',
    noData: 'Chưa có dữ liệu.',
    quickAccess: 'Truy cập nhanh',
    quickAccessChartTitle: 'Tỷ lệ trạng thái (theo tổng)',
    inactive: 'Khác / inactive',
    dashboardAppInfoTitle: 'Thông tin ứng dụng',
    dashboardAppInfoEmpty: 'Chưa có ứng dụng nào.',
    userBan: 'Cấm',
    userUnban: 'Bỏ cấm',
    banUserConfirm: 'Cấm tài khoản này? Họ sẽ không đăng nhập được.',
    viewAll: 'Xem tất cả',
    recentUsers: 'Users gần đây',
    noUsers: 'Chưa có user nào.',
    loading: 'Đang tải…',
    badge: 'CRM · Social Media',
    navOverview: 'Tổng quan',
    navUsers: 'Người dùng',
    navApplications: 'Ứng dụng',
    navRoles: 'Vai trò',
    navSubscriptions: 'Gói đăng ký',
    navPermissions: 'Quyền',
    navRolePermission: 'Role & quyền',
    logout: 'Đăng xuất',
    searchPlaceholder: 'Tìm kiếm...',
    close: 'Đóng',
    paginationShowing: 'Hiển thị {{from}}–{{to}} / {{total}}',
    paginationPrev: 'Trước',
    paginationNext: 'Sau',
    paginationPerPage: 'Số dòng / trang',
    cancel: 'Hủy',
    save: 'Lưu',
    add: 'Thêm',
    edit: 'Sửa',
    select: 'Chọn',
    description: 'Mô tả',
    type: 'Loại',
    startDate: 'Bắt đầu',
    expiryDate: 'Hết hạn',
    members: 'Thành viên',
    noOptions: 'Không có lựa chọn',
    selectUser: '-- Chọn user --',
    selectApplication: '-- Chọn application --',
    subsPageDesc: 'Xem và quản lý gói đăng ký theo User và Application',
    subsSelectUserApp: 'Chọn User & Application',
    subsLoadSubscription: 'Tải subscription',
    subsActiveSubscription: 'Subscription đang active',
    subsNoActiveSubscription: 'Không có subscription active.',
    subsActivate: 'Kích hoạt',
    subsCancelSubscription: 'Hủy subscription',
    subsHistory: 'Lịch sử',
    subsNoHistory: 'Chưa có lịch sử.',
    subsActivateModalTitle: 'Kích hoạt subscription',
    subsPlanId: 'Subscription Plan ID',
    subsPlanType: 'Loại gói',
    subsProcessing: 'Đang xử lý...',
    subsLoadingUsers: 'Đang tải người dùng...',
    subsAddMember: 'Thêm thành viên',
    subsRemoveMember: 'Gỡ thành viên',
    subsMemberLimit: 'Giới hạn thành viên',
    subsJoinedAt: 'Ngày gia nhập',
    subscriptionCatalogDesc: 'Quản lý các gói dịch vụ cơ sở của hệ thống (Template).',
    packageCatalogDesc:
      'Một application có các gói (giá, quota); mỗi gói chứa nhiều license (entitlement / packet).',
    changePasswordDesc: 'Thay đổi mật khẩu đăng nhập của bạn. Cần nhập đúng mật khẩu hiện tại.',
    permissionsTitle: 'Quyền',
    permissionsDesc: 'Định nghĩa và quản lý quyền hệ thống. Gán cho roles trong Gán quyền.',
    permissionsAdd: 'Thêm quyền',
    permissionsSearchPlaceholder: 'Tìm theo mã, mô tả, danh mục…',
    permissionsAllCategories: 'Tất cả danh mục',
    permissionCategoryLabels: {
      applications: 'Ứng dụng',
      audit: 'Kiểm toán',
      permissions: 'Quyền',
      reports: 'Báo cáo',
      roles: 'Vai trò',
      users: 'Người dùng'
    },
    permissionsLoading: 'Đang tải danh sách quyền…',
    permissionsCode: 'Mã',
    permissionsCategory: 'Danh mục',
    permissionsSystem: 'Hệ thống',
    permissionsScope: 'Phạm vi',
    permissionsYes: 'Có',
    permissionsNo: 'Không',
    permissionsEmpty: 'Chưa có quyền nào. Nhấn',
    permissionsAddNew: 'Thêm quyền',
    permissionsAddModalTitle: 'Thêm quyền mới',
    permissionsResource: 'Tài nguyên',
    permissionsAction: 'Hành động',
    permissionsViewUsers: 'Xem người dùng',
    permissionsCreateNew: 'để tạo mới.',
    permissionsCodePlaceholder: 'users.view',
    permissionsCategoryPlaceholder: 'Users',
    permissionsScopePlaceholder: 'global',
    permissionsDescriptionPlaceholder: 'Cho phep xem danh sach users',
    rolesTitle: 'Vai trò hệ thống',
    rolesPageDesc: 'Quản lý vai trò và quyền toàn hệ thống',
    rolesAssignPermissions: 'Gán quyền',
    rolesAddRole: 'Thêm vai trò',
    rolesSearchPlaceholder: 'Tìm theo tên, mô tả, permissions…',
    rolesName: 'Tên',
    rolesSystem: 'Hệ thống',
    rolesActions: 'Thao tác',
    rolesEdit: 'Sửa',
    rolesDelete: 'Xóa',
    rolesDeleting: 'Đang xóa…',
    rolesSaving: 'Đang lưu…',
    rolesEmpty: 'Chưa có role nào. Nhấn "Thêm role" để tạo mới.',
    rolesAddRoleHint: 'Thêm role',
    rolesAddModalTitle: 'Thêm role mới',
    rolesEditModalTitle: 'Sửa role',
    rolesAddNew: 'để tạo mới.',
    usersPageDesc: 'Quản lý tài khoản người dùng và roles.',
    appsPageDesc: 'Duyệt, tạo và quản lý workspace ứng dụng từ một catalog thống nhất.',
    appsAdd: 'Thêm ứng dụng',
    appsEmpty: 'Chưa có ứng dụng nào. Nhấn',
    appsAddHint: 'Thêm ứng dụng',
    appsAddModalTitle: 'Thêm ứng dụng mới',
    appsDescription: 'Mô tả',
    appsDescriptionPlaceholder: 'Mô tả ứng dụng',
    rolePermDesc: 'Xem roles, danh sách permissions và gán permission cho role (kết nối API).',
    rolePermManageLink: '→ Quản lý Permissions (tạo mới)',
    rolePermSelectRole: 'Chọn role',
    rolePermSelectRoleHint: 'Chọn một role bên trái.',
    rolePermNoRoles: 'Chưa có role.',
    rolePermNoPermissions: 'Không có permission nào.',
    rolePermCreateNew: 'Tạo mới',
    rolePermLoadingRolePerms: 'Đang tải permissions của role…',
    userAdminDesc: 'Quản lý tài khoản, phân quyền và đăng ký gói cho đội ngũ marketing và khách hàng.',
    userAdminQuickView: 'Xem nhanh user, role hiện tại và gói đang sử dụng.',
    userAdminSelectPlan: 'Chọn gói này',
    userAdminAddRoleModal: 'Thêm role mới',
    loginNoAccount: 'Chưa có tài khoản?',
    loginRegister: 'Đăng ký',
    loginNoAccountRegister: 'Chưa có tài khoản? Đăng ký',
    oauthAuthorizeEyebrow: 'OAuth consent',
    oauthAuthorizeTitle: 'Xac nhan cap quyen',
    oauthAuthorizeSubtitle: 'Ung dung duoi day dang yeu cau quyen truy cap de tiep tuc dang nhap.',
    oauthAuthorizeAppLabel: 'Ung dung',
    oauthAuthorizeRedirectLabel: 'Redirect host',
    oauthAuthorizeRequestedScopes: 'Scope duoc yeu cau',
    oauthAuthorizeAlreadyGranted: 'Da cap truoc do',
    oauthAuthorizeNewAccess: 'Can duyet moi',
    oauthAuthorizeLoading: 'Dang xu ly yeu cau uy quyen...',
    oauthAuthorizeRedirecting: 'Dang chuyen huong tro lai ung dung...',
    oauthAuthorizeReviewNote: 'Chi phe duyet neu ban tin tuong ung dung va redirect URI nay.',
    oauthAuthorizeApprove: 'Dong y',
    oauthAuthorizeDeny: 'Tu choi',
    oauthAuthorizeMissingParams: 'Thieu tham so bat buoc: client_id, redirect_uri, scope, code_challenge.',
    oauthAuthorizeFailed: 'Authorize that bai.',
    oauthAuthorizeConsentFailed: 'Consent that bai.',
    oauthAuthorizeGrantedBadge: 'Da co',
    oauthAuthorizeRequestedBadge: 'Moi',
    oauthCallbackTitle: 'OAuth callback',
    oauthCallbackSubtitle: 'He thong dang hoan tat dang nhap va dong bo phien lam viec cua ban.',
    oauthCallbackLoading: 'Dang xu ly callback...',
    oauthCallbackSuccess: 'Token exchange thanh cong.',
    oauthCallbackFailed: 'OAuth token exchange that bai.',
    oauthCallbackMissingContext: 'Thieu OAuth callback context (code/client_id/redirect_uri/code_verifier).',
    oauthCallbackStateMismatch: 'OAuth state mismatch.',
    oauthCallbackAdminOnly: 'Chi tai khoan Admin hoac SuperAdmin moi duoc dang nhap tai day.',
    oauthCallbackLoginFailed: 'SSO login failed:',
    layoutBrandEyebrow: 'Trung tâm điều khiển',
    layoutBrandTitle: 'Social CRM',
    navGroupPrimary: 'Chính',
    navGroupAdmin: 'Quản trị',
    navCatalog: 'Danh mục gói',
    navSecurity: 'Bảo mật',
    headerWorkspaceSuffix: 'không gian làm việc',
    layoutDrawerWorkspaceAction: 'Thao tác workspace',
    ariaCloseNavigation: 'Đóng điều hướng',
    ariaOpenNavigation: 'Mở điều hướng',
    ariaSwitchToDark: 'Chuyển chế độ tối',
    ariaSwitchToLight: 'Chuyển chế độ sáng',
    layoutPageDashboard: 'Bảng điều khiển',
    layoutEyebrowOverview: 'Tổng quan',
    layoutPageUsers: 'Người dùng',
    layoutEyebrowIdentity: 'Danh tính',
    layoutEyebrowUserDetail: 'Không gian user',
    layoutPageApplications: 'Ứng dụng',
    layoutEyebrowAppList: 'Topology truy cập',
    layoutEyebrowAppDetail: 'Workspace ứng dụng',
    layoutPageRoles: 'Vai trò',
    layoutEyebrowAuthModel: 'Mô hình phân quyền',
    layoutPageSubscriptions: 'Đăng ký gói',
    layoutEyebrowCommerce: 'Thương mại & gói',
    layoutPageSubscriptionCatalog: 'Danh mục gói đăng ký',
    layoutEyebrowPlanCatalogShort: 'Danh mục gói',
    layoutPagePackageCatalog: 'Danh mục',
    layoutEyebrowPackages: 'Gói & license',
    layoutPagePermissions: 'Quyền',
    layoutEyebrowAccessControl: 'Kiểm soát truy cập',
    layoutPageRoleMapping: 'Gán role & quyền',
    layoutEyebrowPermAssignment: 'Gán quyền',
    layoutPageSecuritySettings: 'Bảo mật',
    layoutEyebrowAccountSettings: 'Cài đặt tài khoản',
    layoutPageUserAdmin: 'Quản trị user',
    layoutEyebrowCrmWorkspace: 'Không gian CRM',
    layoutPageAdmin: 'Quản trị',
    layoutEyebrowWorkspace: 'Workspace',
    dashLoading: 'Đang tải tổng quan…',
    dashRetry: 'Thử lại',
    dashCardActiveSuffix: 'trong số user hiển thị đang hoạt động',
    dashCardSsoSuffix: 'trong số user hiển thị dùng SSO',
    dashCardAppsSub: 'Workspace trong danh mục',
    dashSsoAccountsTitle: 'Tài khoản SSO',
    dashRegTrend: 'Xu hướng đăng ký',
    dashRegTrendDesc: 'Tài khoản mới trong 7 ngày gần nhất.',
    dashDaysLabel: 'ngày',
    dashAccountHealth: 'Sức khỏe tài khoản',
    dashAccountHealthDesc: 'Tỷ lệ nhanh từ dữ liệu user hiện tại.',
    dashRolesWord: 'vai trò',
    dashActiveAccounts: 'Đang hoạt động',
    dashEmailVerified: 'Email đã xác minh',
    dashBannedAccounts: 'Bị cấm',
    dashSsoCoverage: 'Phủ SSO',
    dashAppCatalogTitle: 'Danh mục ứng dụng',
    dashAppCatalogDesc: 'Workspace ứng dụng mới nhất dành cho quản trị.',
    dashViewAll: 'Xem tất cả',
    dashNoDescription: 'Chưa có mô tả',
    dashRecentUsersTitle: 'User gần đây',
    dashRecentUsersDesc: 'User mới nhất từ trang API hiện tại.',
    dashManageUsers: 'Quản lý user',
    dashEmptyApps: 'Chưa có ứng dụng trong danh mục.',
    dashEmptyUsers: 'Chưa có user.',
    dashBannedBadge: 'Đã cấm',
    authConsoleTitle: "Đăng nhập trang quản trị",
    authConsoleSubtitle: "Chỉ dành cho tài khoản Admin và SuperAdmin.",
    authEmail: "Email công việc",
    authPassword: "Mật khẩu",
    authShowPassword: "Hiện mật khẩu",
    authHidePassword: "Ẩn mật khẩu",
    authForgot: "Quên mật khẩu?",
    authSignIn: "Đăng nhập",
    authSigningIn: "Đang đăng nhập…",
    authPasswordTab: "Mật khẩu",
    authMagicTab: "Liên kết email",
    authMagicIntro: "Chúng tôi sẽ gửi liên kết đăng nhập dùng một lần, hết hạn sau 15 phút.",
    authMagicSend: "Gửi liên kết đăng nhập",
    authMagicSending: "Đang gửi…",
    authMagicSent: "Nếu email tồn tại, liên kết đăng nhập đã được gửi.",
    authOr: "hoặc",
    authSsoButton: "Tiếp tục với SSO",
    authSsoMissingConfig: "SSO chưa được cấu hình (client id / redirect URI).",
    authInvalidEmail: "Vui lòng nhập email hợp lệ.",
    authPasswordRequired: "Vui lòng nhập mật khẩu.",
    authAdminOnly: "Trang này chỉ dành cho Admin và SuperAdmin.",
    authSessionExpired: "Phiên đăng nhập đã kết thúc. Vui lòng đăng nhập lại.",
    authSignedIn: "Đăng nhập thành công.",
    authFailed: "Đăng nhập thất bại.",
    authSecureFooter: "Bảo vệ bằng MFA, refresh token xoay vòng và nhật ký kiểm toán.",
    authHeroTitle: "Một lớp định danh cho mọi ứng dụng.",
    authHeroBody: "Đăng nhập tập trung, xác thực đa yếu tố, quản lý phiên và chính sách truy cập cho mọi ứng dụng.",
    authHeroPoint1: "OAuth 2.0 authorization code với PKCE",
    authHeroPoint2: "Xác thực đa yếu tố TOTP và mã khôi phục",
    authHeroPoint3: "Phiên theo thiết bị, thu hồi tức thì",
    mfaTitle: "Xác thực hai lớp",
    mfaSubtitle: "Nhập mã 6 số từ ứng dụng xác thực.",
    mfaRecoverySubtitle: "Nhập một mã khôi phục. Mỗi mã chỉ dùng một lần.",
    mfaCode: "Mã xác thực",
    mfaRecoveryCode: "Mã khôi phục",
    mfaVerify: "Xác minh",
    mfaVerifying: "Đang xác minh…",
    mfaUseRecovery: "Dùng mã khôi phục",
    mfaUseAuthenticator: "Dùng ứng dụng xác thực",
    mfaBack: "Quay lại đăng nhập",
    mfaSigningInAs: "Đang đăng nhập với",
    mfaExpired: "Lượt đăng nhập đã hết hạn. Vui lòng thử lại.",
    ssoPortalTitle: "Đăng nhập để tiếp tục",
    ssoPortalSubtitle: "Dùng tài khoản tổ chức để truy cập các ứng dụng đã kết nối.",
    ssoContinuingTo: "Bạn sẽ được chuyển về ứng dụng sau khi đăng nhập.",
    registerLink: "Tạo tài khoản",
    registerTitle: "Tạo tài khoản",
    registerSubtitle: "Quản trị viên sẽ cấp quyền ứng dụng sau khi bạn xác minh email.",
    registerFirstName: "Tên",
    registerLastName: "Họ",
    registerUsername: "Tên đăng nhập",
    registerDob: "Ngày sinh",
    registerSubmit: "Tạo tài khoản",
    registerSubmitting: "Đang tạo tài khoản…",
    registerDone: "Đã tạo tài khoản. Vui lòng kiểm tra email để xác minh.",
    registerHaveAccount: "Đã có tài khoản?",
    passwordPolicyHint: "Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.",
    forgotTitle: "Đặt lại mật khẩu",
    forgotSubtitle: "Nhập email tài khoản, chúng tôi sẽ gửi liên kết đặt lại.",
    forgotSubmit: "Gửi liên kết",
    forgotSent: "Nếu email tồn tại, bạn sẽ sớm nhận được liên kết đặt lại.",
    resetTitle: "Chọn mật khẩu mới",
    resetSubtitle: "Đặt lại mật khẩu sẽ đăng xuất bạn khỏi mọi thiết bị.",
    resetNewPassword: "Mật khẩu mới",
    resetConfirmPassword: "Xác nhận mật khẩu mới",
    resetSubmit: "Cập nhật mật khẩu",
    resetDone: "Đã cập nhật mật khẩu. Bạn có thể đăng nhập.",
    resetMismatch: "Mật khẩu không khớp.",
    resetInvalidLink: "Liên kết không hợp lệ. Vui lòng yêu cầu liên kết mới.",
    verifyTitle: "Xác minh email",
    verifyWorking: "Đang xác minh email…",
    verifyDone: "Email của bạn đã được xác minh.",
    verifyFailed: "Liên kết xác minh không hợp lệ hoặc đã hết hạn.",
    magicWorking: "Đang đăng nhập…",
    magicFailed: "Liên kết đăng nhập không hợp lệ, đã hết hạn hoặc đã được dùng.",
    backToSignIn: "Quay lại đăng nhập",
    dashTitle: "Tổng quan",
    dashSubtitle: "Định danh, đăng nhập và tình trạng bảo mật của tổ chức.",
    dashRefresh: "Làm mới",
    dashKpiUsers: "Tổng người dùng",
    dashKpiUsersSub: "{{n}} mới trong 30 ngày",
    dashKpiSessions: "Phiên đang hoạt động",
    dashKpiSessionsSub: "Trên mọi thiết bị và ứng dụng",
    dashKpiMfa: "Tỷ lệ bật MFA",
    dashKpiMfaSub: "{{n}}/{{total}} người dùng đã bật",
    dashKpiApps: "Ứng dụng",
    dashKpiAppsSub: "Ứng dụng SSO đang hoạt động",
    dashLoginActivity: "Hoạt động đăng nhập",
    dashLoginActivityDesc: "Đăng nhập thành công và thất bại trong 14 ngày qua.",
    dashSucceeded: "Thành công",
    dashFailed: "Thất bại",
    dashChart: "Biểu đồ",
    dashTable: "Bảng",
    dashIdentityHealth: "Tình trạng tài khoản",
    dashIdentityHealthDesc: "Trạng thái tài khoản trong thư mục.",
    dashActive: "Hoạt động",
    dashVerified: "Đã xác minh email",
    dashLocked: "Bị khóa tạm",
    dashBanned: "Bị cấm",
    dashSignedIn30: "Đăng nhập (30 ngày)",
    dashRecentEvents: "Sự kiện bảo mật gần đây",
    dashViewAuditLog: "Xem nhật ký",
    dashNoEvents: "Chưa có sự kiện nào.",
    dashRecentUsers: "Người dùng mới",
    dashLoadFailed: "Không thể tải tổng quan.",
    dashMfaNudge: "Dưới một nửa người dùng bật MFA. Hãy khuyến khích bật trong trang tài khoản.",
    statusSucceeded: "Thành công",
    statusFailed: "Thất bại",
    accTitle: "Tài khoản của tôi",
    accSubtitle: "Quản lý hồ sơ, phương thức đăng nhập và phiên hoạt động.",
    accTabProfile: "Hồ sơ",
    accTabSecurity: "Bảo mật",
    accTabSessions: "Phiên & thiết bị",
    accTabActivity: "Lịch sử đăng nhập",
    accUsername: "Tên đăng nhập",
    accEmail: "Email",
    accRoles: "Vai trò",
    accMemberSince: "Tham gia từ",
    accEmailStatus: "Trạng thái email",
    accVerified: "Đã xác minh",
    accUnverified: "Chưa xác minh",
    accAccountType: "Loại tài khoản",
    accSso: "Tài khoản SSO",
    accConsole: "Tài khoản quản trị",
    accPasswordTitle: "Mật khẩu",
    accPasswordDesc: "Đổi mật khẩu sẽ đăng xuất các thiết bị khác.",
    accCurrentPassword: "Mật khẩu hiện tại",
    accChangePassword: "Đổi mật khẩu",
    accPasswordChanged: "Đã đổi mật khẩu. Các thiết bị khác đã được đăng xuất.",
    accMfaTitle: "Xác thực hai lớp",
    accMfaDesc: "Yêu cầu mã từ ứng dụng xác thực khi đăng nhập.",
    accMfaOn: "Đã bật",
    accMfaOff: "Chưa bật",
    accMfaEnable: "Thiết lập ứng dụng xác thực",
    accMfaScan: "1. Quét mã QR bằng ứng dụng xác thực",
    accMfaManual: "Không quét được? Nhập khóa thủ công:",
    accMfaEnterCode: "2. Nhập mã 6 số hiển thị",
    accMfaConfirm: "Xác minh và bật",
    accMfaEnabled: "Đã bật xác thực hai lớp. Các thiết bị khác đã được đăng xuất.",
    accMfaDisable: "Tắt",
    accMfaDisableDesc: "Xác nhận bằng mật khẩu và mã hiện tại (hoặc mã khôi phục).",
    accMfaDisabled: "Đã tắt xác thực hai lớp.",
    accRecoveryTitle: "Mã khôi phục",
    accRecoveryDesc: "Dùng khi mất thiết bị. Mỗi mã chỉ dùng một lần — hãy lưu ở nơi an toàn. Mã sẽ không hiển thị lại.",
    accRecoveryRemaining: "Còn {{n}} mã khôi phục",
    accRecoveryRegenerate: "Tạo mã mới",
    accRecoveryCopy: "Sao chép",
    accRecoveryDownload: "Tải xuống",
    accRecoveryDone: "Tôi đã lưu",
    accCopied: "Đã sao chép.",
    accCodeFromApp: "Mã từ ứng dụng xác thực",
    accSessionsDesc: "Thiết bị và trình duyệt đang đăng nhập.",
    accSignOutOthers: "Đăng xuất phiên khác",
    accSignOutOthersConfirm: "Đăng xuất mọi thiết bị khác? Các ứng dụng liên quan cũng sẽ được thông báo.",
    accThisDevice: "Thiết bị này",
    accRevoke: "Đăng xuất",
    accRevokeConfirm: "Đăng xuất phiên này?",
    accLastActive: "Hoạt động lần cuối",
    accSignedIn: "Đăng nhập lúc",
    accExpires: "Hết hạn",
    accKindConsole: "Trang quản trị",
    accKindSso: "Đăng nhập một lần",
    accNoSessions: "Không có phiên nào.",
    accActivityDesc: "Các sự kiện xác thực và bảo mật gần đây.",
    accNoActivity: "Chưa có hoạt động.",
    colEvent: "Sự kiện",
    colResult: "Kết quả",
    colIp: "Địa chỉ IP",
    colDevice: "Thiết bị",
    colWhen: "Thời gian",
    colActor: "Người thực hiện",
    colTarget: "Đối tượng",
    auditTitle: "Nhật ký kiểm toán",
    auditSubtitle: "Mọi sự kiện xác thực, phiên và quản trị, kèm IP, thiết bị và mã tương quan.",
    auditExport: "Xuất CSV",
    auditSearch: "Tìm email, IP, sự kiện…",
    auditAllCategories: "Tất cả nhóm",
    auditAllResults: "Tất cả kết quả",
    auditOnlySucceeded: "Chỉ thành công",
    auditOnlyFailed: "Chỉ thất bại",
    auditFrom: "Từ",
    auditTo: "Đến",
    auditClear: "Xóa bộ lọc",
    auditEmpty: "Không có sự kiện phù hợp.",
    auditDetail: "Chi tiết sự kiện",
    auditCatAuth: "Xác thực",
    auditCatSession: "Phiên",
    auditCatMfa: "MFA",
    auditCatOauth: "OAuth",
    auditCatUser: "Người dùng",
    auditCatApplication: "Ứng dụng",
    auditCatSecurity: "Bảo mật nền tảng",
    auditField: "Trường",
    auditReason: "Lý do",
    auditCorrelation: "Mã tương quan",
    auditSession: "Phiên",
    auditApplication: "Ứng dụng",
    auditUserAgent: "User agent",
    auditDetailsJson: "Dữ liệu bổ sung",
    sysTitle: "Khóa & chính sách",
    sysSubtitle: "Khóa ký token, OpenID discovery và chính sách bảo mật hiện hành.",
    sysKeysTitle: "Khóa ký",
    sysKeysDesc: "Khóa RS256 dùng để ký token. Khóa đã nghỉ vẫn được công bố trong JWKS đến hết thời gian lưu giữ.",
    sysRotate: "Xoay khóa",
    sysRotateConfirm: "Tạo khóa mới và cho khóa hiện tại nghỉ? Token hiện có vẫn hợp lệ đến khi hết hạn.",
    sysRotated: "Đã xoay khóa.",
    sysRevoke: "Thu hồi",
    sysRevokeConfirm: "Thu hồi khóa ngay? Mọi token ký bằng khóa này sẽ mất hiệu lực. Chỉ dùng khi khóa bị lộ.",
    sysRevoked: "Đã thu hồi khóa.",
    sysKeyId: "Mã khóa",
    sysAlgorithm: "Thuật toán",
    sysStatus: "Trạng thái",
    sysCreated: "Tạo lúc",
    sysPublishedUntil: "Công bố đến",
    sysActive: "Đang dùng",
    sysRetired: "Đã nghỉ",
    sysRevokedStatus: "Đã thu hồi",
    sysEndpoints: "Điểm cuối OpenID Connect",
    sysEndpointsDesc: "Cung cấp cho ứng dụng để xác thực token và tìm các điểm cuối.",
    sysIssuer: "Issuer",
    sysAudience: "API audience",
    sysDiscovery: "Tài liệu discovery",
    sysJwks: "JWKS",
    sysCopy: "Sao chép",
    sysPolicy: "Chính sách bảo mật hiện hành",
    sysPolicyDesc: "Giá trị chỉ đọc từ cấu hình máy chủ.",
    sysTokens: "Token & phiên",
    sysAccessToken: "Thời hạn access token",
    sysRefreshIdle: "Hết hạn phiên khi không hoạt động",
    sysSessionAbsolute: "Thời hạn tối đa của phiên",
    sysReuseGrace: "Thời gian ân hạn refresh",
    sysMfaChallenge: "Thời hạn bước MFA",
    sysKeyRotation: "Tự động xoay khóa",
    sysKeyRetention: "Lưu giữ khóa đã nghỉ",
    sysProtection: "Bảo vệ đăng nhập",
    sysPasswordPolicy: "Chính sách mật khẩu",
    sysLockout: "Khóa tài khoản",
    sysRateLimit: "Giới hạn API",
    sysAuthRateLimit: "Giới hạn điểm đăng nhập",
    sysAuditRetention: "Lưu giữ nhật ký",
    sysMinutes: "{{n}} phút",
    sysDays: "{{n}} ngày",
    sysSeconds: "{{n}} giây",
    sysPerWindow: "{{n}} yêu cầu / {{w}} giây mỗi IP",
    sysLockoutValue: "{{n}} lần sai → khóa {{m}} phút",
    sysDisabled: "Tắt",
    sysNoPermission: "Bạn cần quyền SECURITY.READ để xem trang này.",
    usersTitle: "Người dùng",
    usersSubtitle: "Tất cả tài khoản — quản trị viên và tài khoản SSO.",
    usersSearch: "Tìm theo email hoặc tên đăng nhập…",
    usersNewSso: "Tạo người dùng SSO",
    usersColUser: "Người dùng",
    usersColStatus: "Trạng thái",
    usersColSecurity: "Bảo mật",
    usersColType: "Loại",
    usersColLastSignIn: "Đăng nhập gần nhất",
    usersColJoined: "Tham gia",
    usersNever: "Chưa từng",
    usersBanned: "Bị cấm",
    usersInactive: "Không hoạt động",
    usersActive: "Hoạt động",
    usersLocked: "Đang khóa",
    usersUnverified: "Chưa xác minh",
    usersNoMfa: "Chưa MFA",
    usersEmpty: "Không tìm thấy người dùng.",
    usersStandard: "Quản trị",
    usersSso: "SSO",
    usersCreateSsoInfo: "Tạo tài khoản SSO được quản lý. Cấp quyền ứng dụng tại trang người dùng sau đó.",
    usersRequired: "Email, mật khẩu, tên đăng nhập và ngày sinh là bắt buộc.",
    usersCreated: "Đã tạo người dùng SSO.",
    creating: "Đang tạo…",
    create: "Tạo",
    udBack: "Tất cả người dùng",
    udCopyEmail: "Sao chép email",
    udCopyId: "Sao chép ID",
    udDisable: "Cấm người dùng",
    udEnable: "Khôi phục truy cập",
    udRoles: "Vai trò hệ thống",
    udRolesDesc: "Vai trò cấp quyền trên toàn nền tảng.",
    udAddRole: "Thêm vai trò",
    udSelectRole: "Chọn vai trò…",
    udNoRoles: "Chưa có vai trò.",
    udRemoveRole: "Gỡ vai trò",
    udApps: "Quyền truy cập ứng dụng",
    udAppsDesc: "Các ứng dụng tài khoản SSO có thể đăng nhập và vai trò tương ứng.",
    udGrant: "Cấp quyền",
    udSelectApp: "Chọn ứng dụng…",
    udSelectAppRole: "Chọn vai trò…",
    udNoApps: "Chưa được cấp ứng dụng nào.",
    udNoAppsAvailable: "Chưa có ứng dụng nào.",
    udRevoke: "Thu hồi",
    udSecurity: "Bảo mật",
    udSecurityDesc: "Xác thực đa yếu tố và thiết bị đang đăng nhập.",
    udMfaReset: "Đặt lại MFA",
    udMfaResetConfirm: "Xóa ứng dụng xác thực và mã khôi phục của người dùng? Họ sẽ bị đăng xuất khỏi mọi nơi.",
    udMfaResetDone: "Đã đặt lại MFA.",
    udRevokeAll: "Đăng xuất mọi nơi",
    udRevokeAllConfirm: "Kết thúc mọi phiên và thu hồi quyền ứng dụng của người dùng? Các ứng dụng sẽ nhận thông báo đăng xuất.",
    udActiveSessions: "Phiên hoạt động",
    udRecentActivity: "Hoạt động gần đây",
    udBanConfirm: "Cấm người dùng này? Mọi phiên và quyền ứng dụng sẽ bị thu hồi ngay.",
    udUserId: "ID người dùng",
    udBanReason: "Lý do cấm",
    ssoAccountsTitle: "Tài khoản SSO",
    ssoAccountsSubtitle: "Tài khoản người dùng cuối đăng nhập vào các ứng dụng qua SSO.",
    appsTitle: "Ứng dụng",
    appsSubtitle: "Các client OAuth 2.0 / OpenID Connect ủy quyền đăng nhập cho hệ thống này.",
    appsNew: "Ứng dụng mới",
    appsSearch: "Tìm ứng dụng…",
    appsSortName: "Sắp xếp: tên",
    appsSortOwner: "Sắp xếp: chủ sở hữu",
    appsSortBranding: "Sắp xếp: có logo",
    appsGrid: "Dạng lưới",
    appsList: "Dạng danh sách",
    appsEmptyDesc: "Đăng ký ứng dụng đầu tiên để bắt đầu dùng SSO.",
    appsOpen: "Mở",
    appsDelete: "Xóa",
    appsDeleteConfirm: "Xóa \"{{name}}\"? Người dùng sẽ mất quyền và token ngừng hoạt động.",
    appsDeleted: "Đã xóa ứng dụng.",
    appsColApp: "Ứng dụng",
    appsColClientId: "Client id",
    appsColType: "Loại client",
    appsColOwner: "Chủ sở hữu",
    appsName: "Tên",
    appsClientType: "Loại client",
    appsPublic: "Public",
    appsPublicDesc: "SPA và ứng dụng di động. Dùng PKCE, không có secret.",
    appsConfidential: "Confidential",
    appsConfidentialDesc: "Ứng dụng phía máy chủ có thể giữ secret.",
    appsRedirects: "Redirect URI cho phép",
    appsRedirectsHint: "Phân tách bằng dấu phẩy, khớp chính xác. Bắt buộc HTTPS trừ localhost.",
    appsBackchannel: "Back-channel logout URI",
    appsBackchannelHint: "Nhận logout token đã ký khi phiên người dùng kết thúc.",
    appsLogo: "Logo",
    appsUploadLogo: "Tải ảnh",
    appsLogoHint: "PNG, JPG hoặc SVG tối đa 2 MB.",
    appsNameRequired: "Bắt buộc nhập tên.",
    appsCreated: "Đã tạo ứng dụng.",
    appsCredentials: "Thông tin client",
    appsSecretOnce: "Hãy sao chép secret ngay — secret được lưu dạng băm và không thể hiển thị lại.",
    appsClientSecret: "Client secret",
    appsDone: "Xong",
    copied: "Đã sao chép.",
    adBack: "Tất cả ứng dụng",
    adChangeLogo: "Đổi logo",
    adOverview: "Cấu hình client",
    adOverviewDesc: "Định danh và URI đã đăng ký cho client OAuth này.",
    adOwner: "Chủ sở hữu",
    adNoRedirects: "Chưa có redirect URI — yêu cầu ủy quyền sẽ bị từ chối.",
    adNotConfigured: "Chưa cấu hình",
    adCredentials: "Client secret",
    adPublicNote: "Client public xác thực bằng PKCE, không có secret.",
    adRotate: "Xoay secret",
    adLastRotated: "Xoay lần cuối",
    adNeverRotated: "Chưa từng xoay",
    adShow: "Hiện",
    adHide: "Ẩn",
    adIntegration: "Tích hợp",
    adIntegrationDesc: "Authorization code + PKCE (S256). Xác thực token bằng JWKS; nhận back-channel logout.",
    adAuthorizeEndpoint: "Authorization endpoint",
    adTokenEndpoint: "Token endpoint",
    adMembers: "Thành viên",
    adMembersDesc: "Người dùng được phép đăng nhập ứng dụng và vai trò của họ.",
    adAddMember: "Thêm thành viên",
    adRemoveMember: "Gỡ",
    adRemoveMemberConfirm: "Gỡ {{user}} khỏi ứng dụng?",
    adMemberRemoved: "Đã gỡ thành viên.",
    adNoMembers: "Chưa có thành viên.",
    adNeedRole: "Hãy thêm vai trò ứng dụng trước khi thêm thành viên.",
    adUser: "Người dùng",
    adRole: "Vai trò",
    adRoles: "Vai trò ứng dụng",
    adRolesDesc: "Các vai trò dành cho thành viên ứng dụng.",
    adAddRole: "Thêm vai trò",
    adGlobalRole: "Vai trò hệ thống",
    adNoRoles: "Chưa có vai trò.",
    adCapabilities: "Quyền của bạn",
    adCapabilitiesDesc: "Tính năng và quyền của bạn trong ứng dụng (từ vai trò và gói).",
    adFeatures: "Tính năng",
    adPermissions: "Quyền",
    adNone: "Không có",
    adSelectUser: "Chọn người dùng…",
    adSelectRole: "Chọn vai trò…",
    adding: "Đang thêm…",
    rlTitle: "Vai trò",
    rlSubtitle: "Vai trò hệ thống gom nhóm quyền. Gán cho người dùng tại trang người dùng.",
    rlNew: "Vai trò mới",
    rlEdit: "Sửa vai trò",
    rlSearch: "Tìm vai trò hoặc quyền…",
    rlColRole: "Vai trò",
    rlColPermissions: "Quyền",
    rlColUpdated: "Cập nhật",
    rlSystem: "Hệ thống",
    rlMore: "+{{n}} nữa",
    rlEmpty: "Không có vai trò phù hợp.",
    rlName: "Tên",
    rlDescription: "Mô tả",
    rlPermissionsPick: "Chọn quyền…",
    rlPermissionsSearch: "Tìm theo mã hoặc mô tả…",
    rlSystemLocked: "Không thể đổi tên hoặc xóa vai trò hệ thống.",
    rlDeleteConfirm: "Xóa vai trò \"{{name}}\"? Người dùng sẽ mất các quyền của vai trò.",
    rlNameRequired: "Bắt buộc nhập tên vai trò.",
    rlSaved: "Đã lưu vai trò.",
    rlDeleted: "Đã xóa vai trò.",
    saving: "Đang lưu…",
    delete: "Xóa",
    pmTitle: "Quyền",
    pmSubtitle: "Danh mục quyền. Mã theo dạng RESOURCE.ACTION; RESOURCE.* cấp mọi hành động.",
    pmNew: "Quyền mới",
    pmSearch: "Tìm mã, mô tả hoặc nhóm…",
    pmColCode: "Mã",
    pmColCategory: "Nhóm",
    pmColDescription: "Mô tả",
    pmColScope: "OAuth scope",
    pmSystem: "Hệ thống",
    pmEmpty: "Không có quyền phù hợp.",
    pmCodeHint: "Dạng RESOURCE.ACTION viết hoa, ví dụ REPORT.EXPORT",
    pmScopeHint: "OAuth scope tùy chọn tương ứng với quyền.",
    pmCreated: "Đã tạo quyền.",
    rpTitle: "Gán quyền cho vai trò",
    rpSubtitle: "Chọn vai trò để xem và thay đổi quyền.",
    rpSearchRoles: "Lọc vai trò…",
    rpSelectRole: "Chọn một vai trò bên trái để quản lý quyền.",
    rpAssign: "Gán",
    rpPickPermission: "Thêm quyền…",
    rpNoPermissions: "Vai trò chưa có quyền nào.",
    rpAssigned: "Đã gán quyền.",
    rpRemoved: "Đã gỡ quyền.",
    rpRemove: "Gỡ quyền",
    sbTitle: "Gói đăng ký",
    sbSubtitle: "Kích hoạt hoặc hủy gói của thành viên trong ứng dụng. Gói quyết định tính năng và hạn mức.",
    sbApplication: "Ứng dụng",
    sbMember: "Thành viên",
    sbSelectApp: "Chọn ứng dụng…",
    sbSelectMember: "Chọn thành viên…",
    sbPickPrompt: "Chọn ứng dụng và thành viên để xem gói.",
    sbNoMembers: "Ứng dụng chưa có thành viên.",
    sbActive: "Gói hiện tại",
    sbNone: "Không có gói — thành viên không thể dùng ứng dụng.",
    sbActivate: "Kích hoạt gói",
    sbCancel: "Hủy gói",
    sbCancelConfirm: "Hủy gói hiện tại? Thành viên sẽ mất tính năng của gói ngay lập tức.",
    sbCancelled: "Đã hủy gói.",
    sbActivated: "Đã kích hoạt gói.",
    sbType: "Loại",
    sbStarted: "Bắt đầu",
    sbExpires: "Hết hạn",
    sbMembers: "Thành viên",
    sbApiCalls: "Lượt gọi API",
    sbFeatures: "Tính năng",
    sbHistory: "Lịch sử",
    sbNoHistory: "Chưa có lịch sử.",
    sbStatus: "Trạng thái",
    sbStatusActive: "Đang dùng",
    sbStatusCancelled: "Đã hủy",
    sbStatusExpired: "Đã kết thúc",
    sbPlan: "Gói",
    sbChoosePlan: "Chọn gói",
    sbNoPlans: "Chưa có gói cho ứng dụng này. Hãy tạo trong danh mục.",
    sbPickPlan: "Hãy chọn một gói.",
    sbPerMonth: "/ tháng",
    sbUsers: "người dùng",
    ctTitle: "Danh mục gói",
    ctSubtitle: "Gói đăng ký theo ứng dụng: giá, hạn mức và tính năng.",
    ctNew: "Gói mới",
    ctEdit: "Sửa gói",
    ctPickApp: "Chọn ứng dụng để quản lý gói.",
    ctSearch: "Tìm gói…",
    ctEmpty: "Ứng dụng chưa có gói nào.",
    ctMonthly: "Giá tháng (VND)",
    ctYearly: "Giá năm (VND)",
    ctMaxUsers: "Số người dùng tối đa",
    ctMaxApiCalls: "Lượt gọi API tối đa",
    ctUnlimitedHint: "Để trống nghĩa là không giới hạn.",
    ctFeatures: "Tính năng đi kèm",
    ctOrder: "Thứ tự hiển thị",
    ctIcon: "Biểu tượng (tên Material Symbol hoặc URL ảnh)",
    ctActive: "Gói có thể được kích hoạt",
    ctDraft: "Không hoạt động",
    ctLive: "Hoạt động",
    ctSave: "Lưu gói",
    ctDeleteConfirm: "Xóa gói \"{{name}}\"? Gói đăng ký hiện có giữ nguyên quyền lợi.",
    ctSaved: "Đã lưu gói.",
    ctDeleted: "Đã xóa gói.",
    ctNameRequired: "Bắt buộc nhập tên gói.",
    ctSave2: "Tiết kiệm {{n}}%/năm",
    ctFeatAnalytics: "Phân tích",
    ctFeatSupport: "Hỗ trợ ưu tiên",
    ctFeatSecurity: "Bảo mật nâng cao",
    ctFeatDomain: "Tên miền riêng",
    ctPerYear: "/ năm"
  },
  en: {
    overview: 'Overview',
    overviewDesc: 'Manage users, permissions and subscription plans for your social media team.',
    totalUsers: 'Total users',
    totalUsersDesc: 'View user list',
    activeUsers: 'Active',
    activeUsersDesc: 'Active & not banned',
    roles: 'Roles',
    rolesDesc: 'Manage permissions',
    subscriptions: 'Subscriptions',
    subscriptionsDesc: 'Plans & upgrades',
    userDistribution: 'User distribution',
    total: 'Total',
    active: 'Active',
    blocked: 'Blocked',
    loginChartTitle: 'User logins (last 7 days)',
    loginCount: 'Logins',
    trend: 'Trend',
    noData: 'No data yet.',
    quickAccess: 'Quick access',
    quickAccessChartTitle: 'Status mix (vs total)',
    inactive: 'Other / inactive',
    dashboardAppInfoTitle: 'Applications',
    dashboardAppInfoEmpty: 'No applications yet.',
    userBan: 'Ban',
    userUnban: 'Unban',
    banUserConfirm: 'Ban this user? They will not be able to sign in.',
    viewAll: 'View all',
    recentUsers: 'Recent users',
    noUsers: 'No users yet.',
    loading: 'Loading…',
    badge: 'CRM · Social Media',
    navOverview: 'Overview',
    navUsers: 'Users',
    navApplications: 'Applications',
    navRoles: 'Roles',
    navSubscriptions: 'Subscriptions',
    navPermissions: 'Permissions',
    navRolePermission: 'Role & Permission',
    logout: 'Logout',
    searchPlaceholder: 'Search...',
    close: 'Close',
    paginationShowing: 'Showing {{from}}–{{to}} of {{total}}',
    paginationPrev: 'Previous',
    paginationNext: 'Next',
    paginationPerPage: 'Rows per page',
    cancel: 'Cancel',
    save: 'Save',
    add: 'Add',
    edit: 'Edit',
    select: 'Select',
    description: 'Description',
    type: 'Type',
    startDate: 'Started',
    expiryDate: 'Expires',
    members: 'Members',
    noOptions: 'No options',
    selectUser: '-- Select user --',
    selectApplication: '-- Select application --',
    subsPageDesc: 'View and manage subscription packages by User and Application',
    subsSelectUserApp: 'Select User & Application',
    subsLoadSubscription: 'Load subscription',
    subsActiveSubscription: 'Active subscription',
    subsNoActiveSubscription: 'No active subscription.',
    subsActivate: 'Activate',
    subsCancelSubscription: 'Cancel subscription',
    subsHistory: 'History',
    subsNoHistory: 'No history yet.',
    subsActivateModalTitle: 'Activate subscription',
    subsPlanId: 'Subscription Plan ID',
    subsPlanType: 'Plan type',
    subsProcessing: 'Processing...',
    subsLoadingUsers: 'Loading users...',
    subsAddMember: 'Add Member',
    subsRemoveMember: 'Remove Member',
    subsMemberLimit: 'Member Limit',
    subsJoinedAt: 'Joined At',
    subscriptionCatalogDesc: 'Manage system base subscription plans (Template).',
    packageCatalogDesc:
      'Each application has plans (pricing, quota); each plan contains licenses (entitlements / packets).',
    changePasswordDesc: 'Change your login password. You must enter your current password correctly.',
    permissionsTitle: 'Permissions',
    permissionsDesc: 'Define and manage system permissions. Assign to roles in Assign Permissions.',
    permissionsAdd: 'Add permission',
    permissionsSearchPlaceholder: 'Search by code, description, category…',
    permissionsAllCategories: 'All categories',
    permissionCategoryLabels: {
      applications: 'Applications',
      audit: 'Audit',
      permissions: 'Permissions',
      reports: 'Reports',
      roles: 'Roles',
      users: 'Users'
    },
    permissionsLoading: 'Loading permissions…',
    permissionsCode: 'Code',
    permissionsCategory: 'Category',
    permissionsSystem: 'System',
    permissionsScope: 'Scope',
    permissionsYes: 'Yes',
    permissionsNo: 'No',
    permissionsEmpty: 'No permissions yet. Click',
    permissionsAddNew: 'Add permission',
    permissionsAddModalTitle: 'Add new permission',
    permissionsResource: 'Resource',
    permissionsAction: 'Action',
    permissionsViewUsers: 'View users',
    permissionsCreateNew: 'to create.',
    permissionsCodePlaceholder: 'users.view',
    permissionsCategoryPlaceholder: 'Users',
    permissionsScopePlaceholder: 'global',
    permissionsDescriptionPlaceholder: 'Allow viewing the users list',
    rolesTitle: 'Global Roles',
    rolesPageDesc: 'Manage system-wide roles and permissions',
    rolesAssignPermissions: 'Assign permissions',
    rolesAddRole: 'Add role',
    rolesSearchPlaceholder: 'Search by name, description, permissions…',
    rolesName: 'Name',
    rolesSystem: 'System',
    rolesActions: 'Actions',
    rolesEdit: 'Edit',
    rolesDelete: 'Delete',
    rolesDeleting: 'Deleting…',
    rolesSaving: 'Saving…',
    rolesEmpty: 'No roles yet. Click "Add role" to create.',
    rolesAddRoleHint: 'Add role',
    rolesAddModalTitle: 'Add new role',
    rolesEditModalTitle: 'Edit role',
    rolesAddNew: 'to create.',
    usersPageDesc: 'Manage user accounts and roles.',
    appsPageDesc: 'Browse, create and curate application workspaces from one consistent catalog.',
    appsAdd: 'Add application',
    appsEmpty: 'No applications yet. Click',
    appsAddHint: 'Add application',
    appsAddModalTitle: 'Add new application',
    appsDescription: 'Description',
    appsDescriptionPlaceholder: 'Application description',
    rolePermDesc: 'View roles, permissions list and assign permissions to roles (API connection).',
    rolePermManageLink: '→ Manage Permissions (create new)',
    rolePermSelectRole: 'Select role',
    rolePermSelectRoleHint: 'Select a role on the left.',
    rolePermNoRoles: 'No roles yet.',
    rolePermNoPermissions: 'No permissions yet.',
    rolePermCreateNew: 'Create new',
    rolePermLoadingRolePerms: 'Loading role permissions…',
    userAdminDesc: 'Manage accounts, permissions and subscription plans for marketing team and customers.',
    userAdminQuickView: 'Quick view of user, current role and active plan.',
    userAdminSelectPlan: 'Select this plan',
    userAdminAddRoleModal: 'Add new role',
    loginNoAccount: "Don't have an account?",
    loginRegister: 'Register',
    loginNoAccountRegister: "Don't have an account? Register",
    oauthAuthorizeEyebrow: 'OAuth consent',
    oauthAuthorizeTitle: 'Review requested access',
    oauthAuthorizeSubtitle: 'The application below is requesting access before sign-in can continue.',
    oauthAuthorizeAppLabel: 'Application',
    oauthAuthorizeRedirectLabel: 'Redirect host',
    oauthAuthorizeRequestedScopes: 'Requested scopes',
    oauthAuthorizeAlreadyGranted: 'Already granted',
    oauthAuthorizeNewAccess: 'Needs approval',
    oauthAuthorizeLoading: 'Processing authorization request...',
    oauthAuthorizeRedirecting: 'Redirecting back to the application...',
    oauthAuthorizeReviewNote: 'Approve only if you trust this application and its redirect destination.',
    oauthAuthorizeApprove: 'Approve',
    oauthAuthorizeDeny: 'Deny',
    oauthAuthorizeMissingParams: 'Missing required parameters: client_id, redirect_uri, scope, code_challenge.',
    oauthAuthorizeFailed: 'Authorization failed.',
    oauthAuthorizeConsentFailed: 'Consent failed.',
    oauthAuthorizeGrantedBadge: 'Granted',
    oauthAuthorizeRequestedBadge: 'New',
    oauthCallbackTitle: 'OAuth callback',
    oauthCallbackSubtitle: 'We are finalising your sign-in session and exchanging the authorization code.',
    oauthCallbackLoading: 'Processing callback...',
    oauthCallbackSuccess: 'Token exchange completed successfully.',
    oauthCallbackFailed: 'OAuth token exchange failed.',
    oauthCallbackMissingContext: 'Missing OAuth callback context (code/client_id/redirect_uri/code_verifier).',
    oauthCallbackStateMismatch: 'OAuth state mismatch.',
    oauthCallbackAdminOnly: 'Only Admin or SuperAdmin accounts can sign in here.',
    oauthCallbackLoginFailed: 'SSO login failed:',
    layoutBrandEyebrow: 'Control center',
    layoutBrandTitle: 'Social CRM',
    navGroupPrimary: 'Primary',
    navGroupAdmin: 'Admin',
    navCatalog: 'Catalog',
    navSecurity: 'Security',
    headerWorkspaceSuffix: 'workspace',
    layoutDrawerWorkspaceAction: 'Workspace action',
    ariaCloseNavigation: 'Close navigation',
    ariaOpenNavigation: 'Open navigation',
    ariaSwitchToDark: 'Switch to dark mode',
    ariaSwitchToLight: 'Switch to light mode',
    layoutPageDashboard: 'Dashboard',
    layoutEyebrowOverview: 'Overview',
    layoutPageUsers: 'Users',
    layoutEyebrowIdentity: 'Identity management',
    layoutEyebrowUserDetail: 'User workspace',
    layoutPageApplications: 'Applications',
    layoutEyebrowAppList: 'Access topology',
    layoutEyebrowAppDetail: 'Application workspace',
    layoutPageRoles: 'Roles',
    layoutEyebrowAuthModel: 'Authorization model',
    layoutPageSubscriptions: 'Subscriptions',
    layoutEyebrowCommerce: 'Commerce and plans',
    layoutPageSubscriptionCatalog: 'Subscription catalog',
    layoutEyebrowPlanCatalogShort: 'Plan catalog',
    layoutPagePackageCatalog: 'Catalog',
    layoutEyebrowPackages: 'Packages and licenses',
    layoutPagePermissions: 'Permissions',
    layoutEyebrowAccessControl: 'Access control',
    layoutPageRoleMapping: 'Role mapping',
    layoutEyebrowPermAssignment: 'Permission assignment',
    layoutPageSecuritySettings: 'Security',
    layoutEyebrowAccountSettings: 'Account settings',
    layoutPageUserAdmin: 'User admin',
    layoutEyebrowCrmWorkspace: 'CRM workspace',
    layoutPageAdmin: 'Admin',
    layoutEyebrowWorkspace: 'Workspace',
    dashLoading: 'Loading overview…',
    dashRetry: 'Retry',
    dashCardActiveSuffix: 'of visible users are active',
    dashCardSsoSuffix: 'of visible users are SSO-based',
    dashCardAppsSub: 'Workspaces in the catalog',
    dashSsoAccountsTitle: 'SSO accounts',
    dashRegTrend: 'Registration trend',
    dashRegTrendDesc: 'New accounts created during the last 7 days.',
    dashDaysLabel: 'days',
    dashAccountHealth: 'Account health',
    dashAccountHealthDesc: 'Quick ratios derived from current user data.',
    dashRolesWord: 'roles',
    dashActiveAccounts: 'Active accounts',
    dashEmailVerified: 'Email verified',
    dashBannedAccounts: 'Banned accounts',
    dashSsoCoverage: 'SSO coverage',
    dashAppCatalogTitle: 'Application catalog',
    dashAppCatalogDesc: 'Most recent application workspaces available for admins.',
    dashViewAll: 'View all',
    dashNoDescription: 'No description',
    dashRecentUsersTitle: 'Recent users',
    dashRecentUsersDesc: 'Newest users discovered from the current API page.',
    dashManageUsers: 'Manage users',
    dashEmptyApps: 'No applications available in the catalog.',
    dashEmptyUsers: 'No users available.',
    dashBannedBadge: 'Banned',
    navGroupOverview: "Overview",
    navGroupIdentity: "Identity",
    navGroupAccess: "Access control",
    navGroupSecurity: "Security",
    navGroupBilling: "Billing",
    navDashboard: "Dashboard",
    navSsoAccounts: "SSO accounts",
    navAuditLogs: "Audit log",
    navSystem: "Keys & policies",
    navAccount: "My account",
    signOut: "Sign out",
    signOutEverywhere: "Sign out of all devices",
    toggleTheme: "Toggle theme",
    themeLight: "Light",
    themeDark: "Dark",
    language: "Language",
    productName: "Enterprise SSO",
    productTagline: "Identity console",
    skipToContent: "Skip to content",
    openMenu: "Open navigation",
    accountMenu: "Account menu",
    authConsoleTitle: "Sign in to the admin console",
    authConsoleSubtitle: "Administrator and SuperAdmin accounts only.",
    authEmail: "Work email",
    authPassword: "Password",
    authShowPassword: "Show password",
    authHidePassword: "Hide password",
    authForgot: "Forgot password?",
    authSignIn: "Sign in",
    authSigningIn: "Signing in…",
    authPasswordTab: "Password",
    authMagicTab: "Email link",
    authMagicIntro: "We'll email you a one-time sign-in link that expires in 15 minutes.",
    authMagicSend: "Send sign-in link",
    authMagicSending: "Sending…",
    authMagicSent: "If an account exists for this email, a sign-in link is on its way.",
    authOr: "or",
    authSsoButton: "Continue with single sign-on",
    authSsoMissingConfig: "Single sign-on is not configured (OAuth client id / redirect URI).",
    authInvalidEmail: "Enter a valid email address.",
    authPasswordRequired: "Enter your password.",
    authAdminOnly: "This console is restricted to Admin and SuperAdmin accounts.",
    authSessionExpired: "Your session ended. Please sign in again.",
    authSignedIn: "Signed in.",
    authFailed: "Sign-in failed.",
    authSecureFooter: "Protected by MFA, rotating refresh tokens and audit logging.",
    authHeroTitle: "One identity layer for every application.",
    authHeroBody: "Centralised sign-in, multi-factor authentication, sessions and access policies for your organisation's applications.",
    authHeroPoint1: "OAuth 2.0 authorization code flow with PKCE",
    authHeroPoint2: "TOTP multi-factor authentication and recovery codes",
    authHeroPoint3: "Per-device sessions with instant revocation",
    mfaTitle: "Two-factor authentication",
    mfaSubtitle: "Enter the 6-digit code from your authenticator app.",
    mfaRecoverySubtitle: "Enter one of your recovery codes. Each code works once.",
    mfaCode: "Authentication code",
    mfaRecoveryCode: "Recovery code",
    mfaVerify: "Verify",
    mfaVerifying: "Verifying…",
    mfaUseRecovery: "Use a recovery code instead",
    mfaUseAuthenticator: "Use authenticator app instead",
    mfaBack: "Back to sign in",
    mfaSigningInAs: "Signing in as",
    mfaExpired: "This sign-in attempt expired. Please start again.",
    ssoPortalTitle: "Sign in to continue",
    ssoPortalSubtitle: "Use your organisation account to access connected applications.",
    ssoContinuingTo: "You'll be returned to the application after signing in.",
    registerLink: "Create an account",
    registerTitle: "Create your account",
    registerSubtitle: "An administrator grants access to applications after you verify your email.",
    registerFirstName: "First name",
    registerLastName: "Last name",
    registerUsername: "Username",
    registerDob: "Date of birth",
    registerSubmit: "Create account",
    registerSubmitting: "Creating account…",
    registerDone: "Account created. Check your inbox to verify your email address.",
    registerHaveAccount: "Already have an account?",
    passwordPolicyHint: "At least 8 characters with upper- and lower-case letters, a number and a symbol.",
    forgotTitle: "Reset your password",
    forgotSubtitle: "Enter your account email and we'll send a reset link.",
    forgotSubmit: "Send reset link",
    forgotSent: "If an account exists for this email, you'll receive a reset link shortly.",
    resetTitle: "Choose a new password",
    resetSubtitle: "Resetting your password signs you out of every device.",
    resetNewPassword: "New password",
    resetConfirmPassword: "Confirm new password",
    resetSubmit: "Update password",
    resetDone: "Password updated. You can now sign in.",
    resetMismatch: "Passwords do not match.",
    resetInvalidLink: "This reset link is invalid or incomplete. Request a new one.",
    verifyTitle: "Email verification",
    verifyWorking: "Verifying your email…",
    verifyDone: "Your email address is verified.",
    verifyFailed: "This verification link is invalid or has expired.",
    magicWorking: "Signing you in…",
    magicFailed: "This sign-in link is invalid, expired or was already used.",
    backToSignIn: "Back to sign in",
    dashTitle: "Dashboard",
    dashSubtitle: "Identity, sign-in and security posture across your organisation.",
    dashRefresh: "Refresh",
    dashKpiUsers: "Total users",
    dashKpiUsersSub: "{{n}} new in the last 30 days",
    dashKpiSessions: "Active sessions",
    dashKpiSessionsSub: "Across all devices and apps",
    dashKpiMfa: "MFA adoption",
    dashKpiMfaSub: "{{n}} of {{total}} users enrolled",
    dashKpiApps: "Applications",
    dashKpiAppsSub: "Active SSO clients",
    dashLoginActivity: "Sign-in activity",
    dashLoginActivityDesc: "Successful and failed sign-ins over the last 14 days.",
    dashSucceeded: "Successful",
    dashFailed: "Failed",
    dashChart: "Chart",
    dashTable: "Table",
    dashIdentityHealth: "Identity health",
    dashIdentityHealthDesc: "Account state across the directory.",
    dashActive: "Active",
    dashVerified: "Email verified",
    dashLocked: "Locked out",
    dashBanned: "Banned",
    dashSignedIn30: "Signed in (30 days)",
    dashRecentEvents: "Recent security events",
    dashViewAuditLog: "View audit log",
    dashNoEvents: "No security events recorded yet.",
    dashRecentUsers: "Newest users",
    dashLoadFailed: "Could not load the dashboard.",
    dashMfaNudge: "Less than half of your users use MFA. Encourage enrolment from the account page.",
    statusSucceeded: "Succeeded",
    statusFailed: "Failed",
    accTitle: "My account",
    accSubtitle: "Manage your profile, sign-in methods and active sessions.",
    accTabProfile: "Profile",
    accTabSecurity: "Security",
    accTabSessions: "Sessions & devices",
    accTabActivity: "Sign-in history",
    accUsername: "Username",
    accEmail: "Email",
    accRoles: "Roles",
    accMemberSince: "Member since",
    accEmailStatus: "Email status",
    accVerified: "Verified",
    accUnverified: "Not verified",
    accAccountType: "Account type",
    accSso: "SSO account",
    accConsole: "Console account",
    accPasswordTitle: "Password",
    accPasswordDesc: "Changing your password signs out your other devices.",
    accCurrentPassword: "Current password",
    accChangePassword: "Change password",
    accPasswordChanged: "Password changed. Other devices were signed out.",
    accMfaTitle: "Two-factor authentication",
    accMfaDesc: "Require a code from an authenticator app (Google Authenticator, 1Password, Microsoft Authenticator…) when signing in.",
    accMfaOn: "Enabled",
    accMfaOff: "Not enabled",
    accMfaEnable: "Set up authenticator",
    accMfaScan: "1. Scan this QR code with your authenticator app",
    accMfaManual: "Can't scan? Enter this key manually:",
    accMfaEnterCode: "2. Enter the 6-digit code it shows",
    accMfaConfirm: "Verify and enable",
    accMfaEnabled: "Two-factor authentication is on. Other devices were signed out.",
    accMfaDisable: "Turn off",
    accMfaDisableDesc: "Confirm with your password and a current code (or recovery code).",
    accMfaDisabled: "Two-factor authentication was turned off.",
    accRecoveryTitle: "Recovery codes",
    accRecoveryDesc: "Use these if you lose your device. Each works once — store them somewhere safe. They will not be shown again.",
    accRecoveryRemaining: "{{n}} recovery codes remaining",
    accRecoveryRegenerate: "Generate new codes",
    accRecoveryCopy: "Copy codes",
    accRecoveryDownload: "Download",
    accRecoveryDone: "I've saved them",
    accCopied: "Copied to clipboard.",
    accCodeFromApp: "Code from your authenticator app",
    accSessionsDesc: "Devices and browsers currently signed in to your account.",
    accSignOutOthers: "Sign out other sessions",
    accSignOutOthersConfirm: "Sign out every other device and browser? Applications signed in through them are notified too.",
    accThisDevice: "This device",
    accRevoke: "Sign out",
    accRevokeConfirm: "Sign out this session?",
    accLastActive: "Last active",
    accSignedIn: "Signed in",
    accExpires: "Expires",
    accKindConsole: "Admin console",
    accKindSso: "Single sign-on",
    accNoSessions: "No active sessions.",
    accActivityDesc: "Recent authentication and security events on your account.",
    accNoActivity: "No activity yet.",
    colEvent: "Event",
    colResult: "Result",
    colIp: "IP address",
    colDevice: "Device",
    colWhen: "When",
    colActor: "Actor",
    colTarget: "Target",
    auditTitle: "Audit log",
    auditSubtitle: "Every authentication, session and administrative security event, with IP, device and correlation id.",
    auditExport: "Export CSV",
    auditSearch: "Search email, IP, event or target…",
    auditAllCategories: "All categories",
    auditAllResults: "All results",
    auditOnlySucceeded: "Succeeded only",
    auditOnlyFailed: "Failed only",
    auditFrom: "From",
    auditTo: "To",
    auditClear: "Clear filters",
    auditEmpty: "No events match these filters.",
    auditDetail: "Event details",
    auditCatAuth: "Authentication",
    auditCatSession: "Sessions",
    auditCatMfa: "MFA",
    auditCatOauth: "OAuth",
    auditCatUser: "Users",
    auditCatApplication: "Applications",
    auditCatSecurity: "Platform security",
    auditField: "Field",
    auditReason: "Reason",
    auditCorrelation: "Correlation id",
    auditSession: "Session",
    auditApplication: "Application",
    auditUserAgent: "User agent",
    auditDetailsJson: "Additional data",
    sysTitle: "Keys & policies",
    sysSubtitle: "Token signing keys, OpenID discovery and the effective security policy.",
    sysKeysTitle: "Signing keys",
    sysKeysDesc: "RS256 keys used to sign access, ID and logout tokens. Retired keys remain published in the JWKS until their retention window ends.",
    sysRotate: "Rotate key",
    sysRotateConfirm: "Create a new signing key and retire the current one? Existing tokens stay valid until they expire.",
    sysRotated: "Signing key rotated.",
    sysRevoke: "Revoke",
    sysRevokeConfirm: "Revoke this key immediately? Every token signed with it stops working at once. Use this only if the key is compromised.",
    sysRevoked: "Signing key revoked.",
    sysKeyId: "Key id",
    sysAlgorithm: "Algorithm",
    sysStatus: "Status",
    sysCreated: "Created",
    sysPublishedUntil: "Published until",
    sysActive: "Active",
    sysRetired: "Retired",
    sysRevokedStatus: "Revoked",
    sysEndpoints: "OpenID Connect endpoints",
    sysEndpointsDesc: "Give these to relying parties so they can validate tokens and discover endpoints.",
    sysIssuer: "Issuer",
    sysAudience: "API audience",
    sysDiscovery: "Discovery document",
    sysJwks: "JWKS",
    sysCopy: "Copy",
    sysPolicy: "Effective security policy",
    sysPolicyDesc: "Read-only values from server configuration.",
    sysTokens: "Tokens & sessions",
    sysAccessToken: "Access token lifetime",
    sysRefreshIdle: "Session idle timeout",
    sysSessionAbsolute: "Session absolute lifetime",
    sysReuseGrace: "Refresh reuse grace",
    sysMfaChallenge: "MFA challenge lifetime",
    sysKeyRotation: "Automatic key rotation",
    sysKeyRetention: "Retired key retention",
    sysProtection: "Sign-in protection",
    sysPasswordPolicy: "Password policy",
    sysLockout: "Account lockout",
    sysRateLimit: "API rate limit",
    sysAuthRateLimit: "Credential endpoint limit",
    sysAuditRetention: "Audit log retention",
    sysMinutes: "{{n}} min",
    sysDays: "{{n}} days",
    sysSeconds: "{{n}} s",
    sysPerWindow: "{{n}} requests / {{w}} s per IP",
    sysLockoutValue: "{{n}} failed attempts → {{m}} min",
    sysDisabled: "Disabled",
    sysNoPermission: "You need the SECURITY.READ permission to view this page.",
    usersTitle: "Users",
    usersSubtitle: "Everyone in the directory — console administrators and SSO accounts.",
    usersSearch: "Search by email or username…",
    usersNewSso: "New SSO user",
    usersColUser: "User",
    usersColStatus: "Status",
    usersColSecurity: "Security",
    usersColType: "Type",
    usersColLastSignIn: "Last sign-in",
    usersColJoined: "Joined",
    usersNever: "Never",
    usersBanned: "Banned",
    usersInactive: "Inactive",
    usersActive: "Active",
    usersLocked: "Locked",
    usersUnverified: "Unverified",
    usersNoMfa: "No MFA",
    usersEmpty: "No users found.",
    usersStandard: "Console",
    usersSso: "SSO",
    usersCreateSsoInfo: "Creates a managed SSO account. Grant it application access from the user's page afterwards.",
    usersRequired: "Email, password, username and date of birth are required.",
    usersCreated: "SSO user created.",
    creating: "Creating…",
    create: "Create",
    udBack: "All users",
    udCopyEmail: "Copy email",
    udCopyId: "Copy user id",
    udDisable: "Ban user",
    udEnable: "Restore access",
    udRoles: "System roles",
    udRolesDesc: "Roles grant permissions across the platform.",
    udAddRole: "Add role",
    udSelectRole: "Select a role…",
    udNoRoles: "No roles assigned.",
    udRemoveRole: "Remove role",
    udApps: "Application access",
    udAppsDesc: "Applications this SSO account can sign in to, and its role in each.",
    udGrant: "Grant access",
    udSelectApp: "Select an application…",
    udSelectAppRole: "Select a role…",
    udNoApps: "No application access granted yet.",
    udNoAppsAvailable: "No applications exist yet.",
    udRevoke: "Revoke",
    udSecurity: "Security",
    udSecurityDesc: "Multi-factor authentication and signed-in devices.",
    udMfaReset: "Reset MFA",
    udMfaResetConfirm: "Remove this user's authenticator and recovery codes? They will be signed out everywhere and must enrol again.",
    udMfaResetDone: "MFA reset.",
    udRevokeAll: "Sign out everywhere",
    udRevokeAllConfirm: "End all of this user's sessions and revoke every application grant? Connected applications receive a back-channel logout.",
    udActiveSessions: "Active sessions",
    udRecentActivity: "Recent activity",
    udBanConfirm: "Ban this user? All sessions and application grants are revoked immediately.",
    udUserId: "User id",
    udBanReason: "Ban reason",
    ssoAccountsTitle: "SSO accounts",
    ssoAccountsSubtitle: "Managed end-user accounts that sign in to connected applications through single sign-on.",
    appsTitle: "Applications",
    appsSubtitle: "OAuth 2.0 / OpenID Connect clients that delegate sign-in to this identity provider.",
    appsNew: "New application",
    appsSearch: "Search applications…",
    appsSortName: "Sort: name",
    appsSortOwner: "Sort: owner",
    appsSortBranding: "Sort: has logo",
    appsGrid: "Grid view",
    appsList: "List view",
    appsEmptyDesc: "Register your first application to start using single sign-on.",
    appsOpen: "Open",
    appsDelete: "Delete",
    appsDeleteConfirm: "Delete \"{{name}}\"? Users lose access and its tokens stop working.",
    appsDeleted: "Application deleted.",
    appsColApp: "Application",
    appsColClientId: "Client id",
    appsColType: "Client type",
    appsColOwner: "Owner",
    appsName: "Name",
    appsClientType: "Client type",
    appsPublic: "Public",
    appsPublicDesc: "SPAs and mobile apps. Uses PKCE; no client secret.",
    appsConfidential: "Confidential",
    appsConfidentialDesc: "Server-side apps that can keep a client secret.",
    appsRedirects: "Allowed redirect URIs",
    appsRedirectsHint: "Comma-separated, exact match. HTTPS required except for localhost.",
    appsBackchannel: "Back-channel logout URI",
    appsBackchannelHint: "Receives a signed logout token when a user's session ends.",
    appsLogo: "Logo",
    appsUploadLogo: "Upload image",
    appsLogoHint: "PNG, JPG or SVG up to 2 MB.",
    appsNameRequired: "Name is required.",
    appsCreated: "Application created.",
    appsCredentials: "Client credentials",
    appsSecretOnce: "Copy the client secret now — it is stored hashed and cannot be shown again. Rotate it if it is lost.",
    appsClientSecret: "Client secret",
    appsDone: "Done",
    copied: "Copied.",
    adBack: "All applications",
    adChangeLogo: "Change logo",
    adOverview: "Client configuration",
    adOverviewDesc: "Identifiers and URIs registered for this OAuth client.",
    adOwner: "Owner",
    adNoRedirects: "No redirect URIs registered — authorization requests will be rejected.",
    adNotConfigured: "Not configured",
    adCredentials: "Client secret",
    adPublicNote: "Public clients authenticate with PKCE and never receive a client secret.",
    adRotate: "Rotate secret",
    adLastRotated: "Last rotated",
    adNeverRotated: "Never rotated",
    adShow: "Show",
    adHide: "Hide",
    adIntegration: "Integration",
    adIntegrationDesc: "Authorization code + PKCE (S256). Validate tokens with the JWKS; listen for back-channel logout.",
    adAuthorizeEndpoint: "Authorization endpoint",
    adTokenEndpoint: "Token endpoint",
    adMembers: "Members",
    adMembersDesc: "Users allowed to sign in to this application and their role in it.",
    adAddMember: "Add member",
    adRemoveMember: "Remove",
    adRemoveMemberConfirm: "Remove {{user}} from this application?",
    adMemberRemoved: "Member removed.",
    adNoMembers: "No members yet.",
    adNeedRole: "Add an application role before adding members.",
    adUser: "User",
    adRole: "Role",
    adRoles: "Application roles",
    adRolesDesc: "Roles available to members of this application.",
    adAddRole: "Add role",
    adGlobalRole: "Global role",
    adNoRoles: "No roles yet.",
    adCapabilities: "Your capabilities",
    adCapabilitiesDesc: "Features and permissions your account has in this application (from roles and subscription).",
    adFeatures: "Features",
    adPermissions: "Permissions",
    adNone: "None",
    adSelectUser: "Select a user…",
    adSelectRole: "Select a role…",
    adding: "Adding…",
    rlTitle: "Roles",
    rlSubtitle: "Global roles bundle permissions. Assign them to users from the user's page.",
    rlNew: "New role",
    rlEdit: "Edit role",
    rlSearch: "Search roles or permissions…",
    rlColRole: "Role",
    rlColPermissions: "Permissions",
    rlColUpdated: "Updated",
    rlSystem: "System",
    rlMore: "+{{n}} more",
    rlEmpty: "No roles match.",
    rlName: "Name",
    rlDescription: "Description",
    rlPermissionsPick: "Choose permissions…",
    rlPermissionsSearch: "Search by code or description…",
    rlSystemLocked: "System roles cannot be renamed or deleted.",
    rlDeleteConfirm: "Delete the role \"{{name}}\"? Users holding it lose its permissions.",
    rlNameRequired: "Role name is required.",
    rlSaved: "Role saved.",
    rlDeleted: "Role deleted.",
    saving: "Saving…",
    delete: "Delete",
    pmTitle: "Permissions",
    pmSubtitle: "The permission catalog. Codes follow RESOURCE.ACTION; RESOURCE.* grants every action.",
    pmNew: "New permission",
    pmSearch: "Search code, description or category…",
    pmColCode: "Code",
    pmColCategory: "Category",
    pmColDescription: "Description",
    pmColScope: "OAuth scope",
    pmSystem: "System",
    pmEmpty: "No permissions match.",
    pmCodeHint: "Uppercase RESOURCE.ACTION, e.g. REPORT.EXPORT",
    pmScopeHint: "Optional OAuth scope that maps to this permission.",
    pmCreated: "Permission created.",
    rpTitle: "Role & permission mapping",
    rpSubtitle: "Pick a role to review and change the permissions it grants.",
    rpSearchRoles: "Filter roles…",
    rpSelectRole: "Select a role on the left to manage its permissions.",
    rpAssign: "Assign",
    rpPickPermission: "Add a permission…",
    rpNoPermissions: "This role grants no permissions.",
    rpAssigned: "Permission assigned.",
    rpRemoved: "Permission removed.",
    rpRemove: "Remove permission",
    sbTitle: "Subscriptions",
    sbSubtitle: "Activate or cancel a member's plan for an application. Plans drive feature entitlements and quotas.",
    sbApplication: "Application",
    sbMember: "Member",
    sbSelectApp: "Select an application…",
    sbSelectMember: "Select a member…",
    sbPickPrompt: "Choose an application and a member to view their subscription.",
    sbNoMembers: "This application has no members yet.",
    sbActive: "Current subscription",
    sbNone: "No active subscription — the member cannot use this application.",
    sbActivate: "Activate plan",
    sbCancel: "Cancel subscription",
    sbCancelConfirm: "Cancel the current subscription? The member loses plan features immediately.",
    sbCancelled: "Subscription cancelled.",
    sbActivated: "Subscription activated.",
    sbType: "Type",
    sbStarted: "Started",
    sbExpires: "Expires",
    sbMembers: "Members",
    sbApiCalls: "API calls",
    sbFeatures: "Features",
    sbHistory: "History",
    sbNoHistory: "No previous subscriptions.",
    sbStatus: "Status",
    sbStatusActive: "Active",
    sbStatusCancelled: "Cancelled",
    sbStatusExpired: "Ended",
    sbPlan: "Plan",
    sbChoosePlan: "Choose a plan",
    sbNoPlans: "No plans are configured for this application. Create one in the catalog.",
    sbPickPlan: "Select a plan first.",
    sbPerMonth: "/ month",
    sbUsers: "users",
    ctTitle: "Plan catalog",
    ctSubtitle: "Subscription plans per application: pricing, quotas and bundled features.",
    ctNew: "New plan",
    ctEdit: "Edit plan",
    ctPickApp: "Choose an application to manage its plans.",
    ctSearch: "Search plans…",
    ctEmpty: "No plans for this application yet.",
    ctMonthly: "Monthly price (VND)",
    ctYearly: "Yearly price (VND)",
    ctMaxUsers: "Max users",
    ctMaxApiCalls: "Max API calls",
    ctUnlimitedHint: "Leave empty for unlimited.",
    ctFeatures: "Included features",
    ctOrder: "Display order",
    ctIcon: "Icon (Material Symbol name or image URL)",
    ctActive: "Plan is available for activation",
    ctDraft: "Inactive",
    ctLive: "Active",
    ctSave: "Save plan",
    ctDeleteConfirm: "Delete the plan \"{{name}}\"? Existing subscriptions keep their entitlement snapshot.",
    ctSaved: "Plan saved.",
    ctDeleted: "Plan deleted.",
    ctNameRequired: "Plan name is required.",
    ctSave2: "Save {{n}}% yearly",
    ctFeatAnalytics: "Analytics",
    ctFeatSupport: "Priority support",
    ctFeatSecurity: "Advanced security",
    ctFeatDomain: "Custom domain",
    ctPerYear: "/ year"
  },
  zh: {
    overview: '概览',
    overviewDesc: '管理用户、权限和社交媒体团队的订阅计划。',
    totalUsers: '总用户数',
    totalUsersDesc: '查看用户列表',
    activeUsers: '活跃',
    activeUsersDesc: '活跃且未被封禁',
    roles: '角色',
    rolesDesc: '管理权限',
    subscriptions: '订阅',
    subscriptionsDesc: '套餐与升级',
    userDistribution: '用户分布',
    total: '总计',
    active: '活跃',
    blocked: '已封禁',
    loginChartTitle: '用户登录（最近7天）',
    loginCount: '登录次数',
    trend: '趋势',
    noData: '暂无数据。',
    quickAccess: '快捷访问',
    quickAccessChartTitle: '状态占比（相对总数）',
    inactive: '其他 / 未激活',
    dashboardAppInfoTitle: '应用信息',
    dashboardAppInfoEmpty: '暂无应用。',
    userBan: '封禁',
    userUnban: '解封',
    banUserConfirm: '封禁此用户？他们将无法登录。',
    viewAll: '查看全部',
    recentUsers: '最近用户',
    noUsers: '暂无用户。',
    loading: '加载中…',
    badge: 'CRM · 社交媒体',
    navOverview: '概览',
    navUsers: '用户',
    navApplications: '应用',
    navRoles: '角色',
    navSubscriptions: '订阅',
    navPermissions: '权限',
    navRolePermission: '角色与权限',
    logout: '登出',
    searchPlaceholder: '搜索...',
    close: '关闭',
    paginationShowing: '显示 {{from}}–{{to}} / 共 {{total}} 条',
    paginationPrev: '上一页',
    paginationNext: '下一页',
    paginationPerPage: '每页行数',
    cancel: '取消',
    save: '保存',
    add: '添加',
    edit: '编辑',
    select: '选择',
    description: '描述',
    type: '类型',
    startDate: '开始',
    expiryDate: '到期',
    members: '成员',
    noOptions: '无选项',
    selectUser: '-- 选择用户 --',
    selectApplication: '-- 选择应用 --',
    subsPageDesc: '按用户和应用查看和管理订阅套餐',
    subsSelectUserApp: '选择用户和应用',
    subsLoadSubscription: '加载订阅',
    subsActiveSubscription: '当前订阅',
    subsNoActiveSubscription: '暂无活跃订阅。',
    subsActivate: '激活',
    subsCancelSubscription: '取消订阅',
    subsHistory: '历史',
    subsNoHistory: '暂无历史。',
    subsActivateModalTitle: '激活订阅',
    subsPlanId: '订阅计划 ID',
    subsPlanType: '套餐类型',
    subsProcessing: '处理中...',
    subsLoadingUsers: '正在加载用户...',
    subsAddMember: '添加成员',
    subsRemoveMember: '移除成员',
    subsMemberLimit: '成员限制',
    subsJoinedAt: '加入时间',
    subscriptionCatalogDesc: '管理系统基础订阅方案（模板）。',
    packageCatalogDesc: '每个应用包含套餐（定价、配额）；每个套餐包含多个许可证（权益/数据包）。',
    changePasswordDesc: '更改登录密码。必须正确输入当前密码。',
    permissionsTitle: '权限',
    permissionsDesc: '定义和管理系统权限。在分配权限中分配给角色。',
    permissionsAdd: '添加权限',
    permissionsSearchPlaceholder: '按代码、描述、类别搜索…',
    permissionsAllCategories: '全部类别',
    permissionCategoryLabels: {
      applications: '应用',
      audit: '审计',
      permissions: '权限',
      reports: '报表',
      roles: '角色',
      users: '用户'
    },
    permissionsLoading: '加载权限中…',
    permissionsCode: '代码',
    permissionsCategory: '类别',
    permissionsSystem: '系统',
    permissionsScope: '范围',
    permissionsYes: '是',
    permissionsNo: '否',
    permissionsEmpty: '暂无权限。点击',
    permissionsAddNew: '添加权限',
    permissionsAddModalTitle: '添加新权限',
    permissionsResource: '资源',
    permissionsAction: '操作',
    permissionsViewUsers: '查看用户',
    permissionsCreateNew: '创建。',
    permissionsCodePlaceholder: 'users.view',
    permissionsCategoryPlaceholder: 'Users',
    permissionsScopePlaceholder: 'global',
    permissionsDescriptionPlaceholder: '允许查看用户列表',
    rolesTitle: '全局角色',
    rolesPageDesc: '管理系统级角色和权限',
    rolesAssignPermissions: '分配权限',
    rolesAddRole: '添加角色',
    rolesSearchPlaceholder: '按名称、描述、权限搜索…',
    rolesName: '名称',
    rolesSystem: '系统',
    rolesActions: '操作',
    rolesEdit: '编辑',
    rolesDelete: '删除',
    rolesDeleting: '删除中…',
    rolesSaving: '保存中…',
    rolesEmpty: '暂无角色。点击"添加角色"创建。',
    rolesAddRoleHint: '添加角色',
    rolesAddModalTitle: '添加新角色',
    rolesEditModalTitle: '编辑角色',
    rolesAddNew: '创建。',
    usersPageDesc: '管理用户账户与角色。',
    appsPageDesc: '在一个统一目录中浏览、创建并管理应用工作区。',
    appsAdd: '添加应用',
    appsEmpty: '暂无应用。点击',
    appsAddHint: '添加应用',
    appsAddModalTitle: '添加新应用',
    appsDescription: '描述',
    appsDescriptionPlaceholder: '应用描述',
    rolePermDesc: '查看角色、权限列表并为角色分配权限（API 连接）。',
    rolePermManageLink: '→ 管理权限（新建）',
    rolePermSelectRole: '选择角色',
    rolePermSelectRoleHint: '在左侧选择一个角色。',
    rolePermNoRoles: '暂无角色。',
    rolePermNoPermissions: '暂无权限。',
    rolePermCreateNew: '新建',
    rolePermLoadingRolePerms: '加载角色权限中…',
    userAdminDesc: '管理营销团队和客户的账户、权限和订阅计划。',
    userAdminQuickView: '快速查看用户、当前角色和活跃套餐。',
    userAdminSelectPlan: '选择此套餐',
    userAdminAddRoleModal: '添加新角色',
    loginNoAccount: '没有账户？',
    loginRegister: '注册',
    loginNoAccountRegister: '没有账户？注册',
    oauthAuthorizeEyebrow: 'OAuth 授权',
    oauthAuthorizeTitle: '确认授权范围',
    oauthAuthorizeSubtitle: '下方应用需要这些权限后才能继续登录流程。',
    oauthAuthorizeAppLabel: '应用',
    oauthAuthorizeRedirectLabel: '回调域名',
    oauthAuthorizeRequestedScopes: '请求的 Scope',
    oauthAuthorizeAlreadyGranted: '已授权',
    oauthAuthorizeNewAccess: '待批准',
    oauthAuthorizeLoading: '正在处理授权请求...',
    oauthAuthorizeRedirecting: '正在跳回应用...',
    oauthAuthorizeReviewNote: '仅在你信任该应用和回调地址时才批准这些权限。',
    oauthAuthorizeApprove: '批准',
    oauthAuthorizeDeny: '拒绝',
    oauthAuthorizeMissingParams: '缺少必填参数：client_id、redirect_uri、scope、code_challenge。',
    oauthAuthorizeFailed: '授权失败。',
    oauthAuthorizeConsentFailed: '授权确认失败。',
    oauthAuthorizeGrantedBadge: '已授权',
    oauthAuthorizeRequestedBadge: '新增',
    oauthCallbackTitle: 'OAuth 回调',
    oauthCallbackSubtitle: '系统正在完成登录并交换授权码。',
    oauthCallbackLoading: '正在处理回调...',
    oauthCallbackSuccess: 'Token 交换成功。',
    oauthCallbackFailed: 'OAuth token exchange failed.',
    oauthCallbackMissingContext: '缺少 OAuth 回调上下文（code/client_id/redirect_uri/code_verifier）。',
    oauthCallbackStateMismatch: 'OAuth state 不匹配。',
    oauthCallbackAdminOnly: '只有 Admin 或 SuperAdmin 账号可在此登录。',
    oauthCallbackLoginFailed: 'SSO login failed:',
    layoutBrandEyebrow: '控制中心',
    layoutBrandTitle: 'Social CRM',
    navGroupPrimary: '主要',
    navGroupAdmin: '管理',
    navCatalog: '目录',
    navSecurity: '安全',
    headerWorkspaceSuffix: '工作区',
    layoutDrawerWorkspaceAction: '工作区操作',
    ariaCloseNavigation: '关闭导航',
    ariaOpenNavigation: '打开导航',
    ariaSwitchToDark: '切换到深色模式',
    ariaSwitchToLight: '切换到浅色模式',
    layoutPageDashboard: '仪表板',
    layoutEyebrowOverview: '概览',
    layoutPageUsers: '用户',
    layoutEyebrowIdentity: '身份管理',
    layoutEyebrowUserDetail: '用户空间',
    layoutPageApplications: '应用',
    layoutEyebrowAppList: '访问拓扑',
    layoutEyebrowAppDetail: '应用工作区',
    layoutPageRoles: '角色',
    layoutEyebrowAuthModel: '授权模型',
    layoutPageSubscriptions: '订阅',
    layoutEyebrowCommerce: '商务与套餐',
    layoutPageSubscriptionCatalog: '订阅目录',
    layoutEyebrowPlanCatalogShort: '套餐目录',
    layoutPagePackageCatalog: '目录',
    layoutEyebrowPackages: '套餐与许可证',
    layoutPagePermissions: '权限',
    layoutEyebrowAccessControl: '访问控制',
    layoutPageRoleMapping: '角色映射',
    layoutEyebrowPermAssignment: '权限分配',
    layoutPageSecuritySettings: '安全',
    layoutEyebrowAccountSettings: '账户设置',
    layoutPageUserAdmin: '用户管理',
    layoutEyebrowCrmWorkspace: 'CRM 工作区',
    layoutPageAdmin: '管理',
    layoutEyebrowWorkspace: '工作区',
    dashLoading: '正在加载概览…',
    dashRetry: '重试',
    dashCardActiveSuffix: '的可见用户处于活跃状态',
    dashCardSsoSuffix: '的可见用户使用 SSO',
    dashCardAppsSub: '目录中的工作区',
    dashSsoAccountsTitle: 'SSO 账户',
    dashRegTrend: '注册趋势',
    dashRegTrendDesc: '最近 7 天新创建的账户。',
    dashDaysLabel: '天',
    dashAccountHealth: '账户健康',
    dashAccountHealthDesc: '基于当前用户数据的快速比例。',
    dashRolesWord: '个角色',
    dashActiveAccounts: '活跃账户',
    dashEmailVerified: '邮箱已验证',
    dashBannedAccounts: '已封禁',
    dashSsoCoverage: 'SSO 覆盖',
    dashAppCatalogTitle: '应用目录',
    dashAppCatalogDesc: '管理员可用的最新应用工作区。',
    dashViewAll: '查看全部',
    dashNoDescription: '暂无描述',
    dashRecentUsersTitle: '最近用户',
    dashRecentUsersDesc: '当前 API 页发现的最新用户。',
    dashManageUsers: '管理用户',
    dashEmptyApps: '目录中暂无应用。',
    dashEmptyUsers: '暂无用户。',
    dashBannedBadge: '已封禁',
    authConsoleTitle: "登录管理控制台",
    authConsoleSubtitle: "仅限 Admin 与 SuperAdmin 账户。",
    authEmail: "工作邮箱",
    authPassword: "密码",
    authShowPassword: "显示密码",
    authHidePassword: "隐藏密码",
    authForgot: "忘记密码？",
    authSignIn: "登录",
    authSigningIn: "正在登录…",
    authPasswordTab: "密码",
    authMagicTab: "邮件链接",
    authMagicIntro: "我们将发送一次性登录链接，15 分钟后失效。",
    authMagicSend: "发送登录链接",
    authMagicSending: "发送中…",
    authMagicSent: "如果该邮箱存在账户，登录链接已发送。",
    authOr: "或",
    authSsoButton: "使用单点登录继续",
    authSsoMissingConfig: "单点登录未配置（client id / redirect URI）。",
    authInvalidEmail: "请输入有效邮箱。",
    authPasswordRequired: "请输入密码。",
    authAdminOnly: "此控制台仅限 Admin 与 SuperAdmin。",
    authSessionExpired: "会话已结束，请重新登录。",
    authSignedIn: "登录成功。",
    authFailed: "登录失败。",
    authSecureFooter: "由 MFA、轮换刷新令牌和审计日志保护。",
    authHeroTitle: "为每个应用提供统一身份层。",
    authHeroBody: "集中登录、多因素认证、会话与访问策略管理。",
    authHeroPoint1: "OAuth 2.0 授权码 + PKCE",
    authHeroPoint2: "TOTP 多因素认证与恢复码",
    authHeroPoint3: "按设备会话，可即时撤销",
    mfaTitle: "双重验证",
    mfaSubtitle: "输入身份验证器应用中的 6 位代码。",
    mfaRecoverySubtitle: "输入一个恢复码，每个仅可使用一次。",
    mfaCode: "验证码",
    mfaRecoveryCode: "恢复码",
    mfaVerify: "验证",
    mfaVerifying: "验证中…",
    mfaUseRecovery: "改用恢复码",
    mfaUseAuthenticator: "改用身份验证器",
    mfaBack: "返回登录",
    mfaSigningInAs: "正在登录",
    mfaExpired: "本次登录已过期，请重新开始。",
    ssoPortalTitle: "登录以继续",
    ssoPortalSubtitle: "使用组织账户访问已连接的应用。",
    ssoContinuingTo: "登录后将返回应用。",
    registerLink: "创建账户",
    registerTitle: "创建账户",
    registerSubtitle: "验证邮箱后，管理员将为您授予应用访问权限。",
    registerFirstName: "名",
    registerLastName: "姓",
    registerUsername: "用户名",
    registerDob: "出生日期",
    registerSubmit: "创建账户",
    registerSubmitting: "正在创建…",
    registerDone: "账户已创建，请查收邮件完成验证。",
    registerHaveAccount: "已有账户？",
    passwordPolicyHint: "至少 8 位，包含大小写字母、数字和符号。",
    forgotTitle: "重置密码",
    forgotSubtitle: "输入账户邮箱，我们将发送重置链接。",
    forgotSubmit: "发送重置链接",
    forgotSent: "如果该邮箱存在账户，您将很快收到重置链接。",
    resetTitle: "设置新密码",
    resetSubtitle: "重置密码将使您在所有设备上退出登录。",
    resetNewPassword: "新密码",
    resetConfirmPassword: "确认新密码",
    resetSubmit: "更新密码",
    resetDone: "密码已更新，现在可以登录。",
    resetMismatch: "两次密码不一致。",
    resetInvalidLink: "重置链接无效，请重新申请。",
    verifyTitle: "邮箱验证",
    verifyWorking: "正在验证邮箱…",
    verifyDone: "邮箱已验证。",
    verifyFailed: "验证链接无效或已过期。",
    magicWorking: "正在登录…",
    magicFailed: "登录链接无效、已过期或已使用。",
    backToSignIn: "返回登录",
    dashTitle: "仪表板",
    dashSubtitle: "组织的身份、登录与安全态势。",
    dashRefresh: "刷新",
    dashKpiUsers: "用户总数",
    dashKpiUsersSub: "近 30 天新增 {{n}}",
    dashKpiSessions: "活跃会话",
    dashKpiSessionsSub: "所有设备与应用",
    dashKpiMfa: "MFA 启用率",
    dashKpiMfaSub: "{{total}} 中 {{n}} 人已启用",
    dashKpiApps: "应用",
    dashKpiAppsSub: "活跃 SSO 客户端",
    dashLoginActivity: "登录活动",
    dashLoginActivityDesc: "近 14 天成功与失败的登录。",
    dashSucceeded: "成功",
    dashFailed: "失败",
    dashChart: "图表",
    dashTable: "表格",
    dashIdentityHealth: "身份健康",
    dashIdentityHealthDesc: "目录中的账户状态。",
    dashActive: "活跃",
    dashVerified: "邮箱已验证",
    dashLocked: "已锁定",
    dashBanned: "已封禁",
    dashSignedIn30: "登录（30 天）",
    dashRecentEvents: "近期安全事件",
    dashViewAuditLog: "查看审计日志",
    dashNoEvents: "暂无安全事件。",
    dashRecentUsers: "最新用户",
    dashLoadFailed: "无法加载仪表板。",
    dashMfaNudge: "不到一半的用户启用了 MFA。",
    statusSucceeded: "成功",
    statusFailed: "失败",
    accTitle: "我的账户",
    accSubtitle: "管理个人资料、登录方式和活跃会话。",
    accTabProfile: "资料",
    accTabSecurity: "安全",
    accTabSessions: "会话与设备",
    accTabActivity: "登录历史",
    accUsername: "用户名",
    accEmail: "邮箱",
    accRoles: "角色",
    accMemberSince: "注册时间",
    accEmailStatus: "邮箱状态",
    accVerified: "已验证",
    accUnverified: "未验证",
    accAccountType: "账户类型",
    accSso: "SSO 账户",
    accConsole: "控制台账户",
    accPasswordTitle: "密码",
    accPasswordDesc: "修改密码将退出其他设备。",
    accCurrentPassword: "当前密码",
    accChangePassword: "修改密码",
    accPasswordChanged: "密码已修改，其他设备已退出。",
    accMfaTitle: "双重验证",
    accMfaDesc: "登录时需要身份验证器应用中的代码。",
    accMfaOn: "已启用",
    accMfaOff: "未启用",
    accMfaEnable: "设置身份验证器",
    accMfaScan: "1. 用身份验证器扫描二维码",
    accMfaManual: "无法扫描？手动输入此密钥：",
    accMfaEnterCode: "2. 输入显示的 6 位代码",
    accMfaConfirm: "验证并启用",
    accMfaEnabled: "已启用双重验证，其他设备已退出。",
    accMfaDisable: "关闭",
    accMfaDisableDesc: "请输入密码和当前代码（或恢复码）确认。",
    accMfaDisabled: "已关闭双重验证。",
    accRecoveryTitle: "恢复码",
    accRecoveryDesc: "设备丢失时使用，每个仅可用一次，请妥善保存，之后不会再次显示。",
    accRecoveryRemaining: "剩余 {{n}} 个恢复码",
    accRecoveryRegenerate: "生成新代码",
    accRecoveryCopy: "复制",
    accRecoveryDownload: "下载",
    accRecoveryDone: "我已保存",
    accCopied: "已复制。",
    accCodeFromApp: "身份验证器中的代码",
    accSessionsDesc: "当前登录您账户的设备与浏览器。",
    accSignOutOthers: "退出其他会话",
    accSignOutOthersConfirm: "退出所有其他设备？相关应用也会收到通知。",
    accThisDevice: "本设备",
    accRevoke: "退出",
    accRevokeConfirm: "退出此会话？",
    accLastActive: "最近活动",
    accSignedIn: "登录于",
    accExpires: "过期",
    accKindConsole: "管理控制台",
    accKindSso: "单点登录",
    accNoSessions: "没有活跃会话。",
    accActivityDesc: "账户近期的认证与安全事件。",
    accNoActivity: "暂无活动。",
    colEvent: "事件",
    colResult: "结果",
    colIp: "IP 地址",
    colDevice: "设备",
    colWhen: "时间",
    colActor: "操作者",
    colTarget: "目标",
    auditTitle: "审计日志",
    auditSubtitle: "所有认证、会话和管理安全事件，含 IP、设备和关联 ID。",
    auditExport: "导出 CSV",
    auditSearch: "搜索邮箱、IP、事件…",
    auditAllCategories: "所有类别",
    auditAllResults: "所有结果",
    auditOnlySucceeded: "仅成功",
    auditOnlyFailed: "仅失败",
    auditFrom: "从",
    auditTo: "至",
    auditClear: "清除筛选",
    auditEmpty: "没有符合条件的事件。",
    auditDetail: "事件详情",
    auditCatAuth: "认证",
    auditCatSession: "会话",
    auditCatMfa: "MFA",
    auditCatOauth: "OAuth",
    auditCatUser: "用户",
    auditCatApplication: "应用",
    auditCatSecurity: "平台安全",
    auditField: "字段",
    auditReason: "原因",
    auditCorrelation: "关联 ID",
    auditSession: "会话",
    auditApplication: "应用",
    auditUserAgent: "User agent",
    auditDetailsJson: "附加数据",
    sysTitle: "密钥与策略",
    sysSubtitle: "令牌签名密钥、OpenID 发现与当前安全策略。",
    sysKeysTitle: "签名密钥",
    sysKeysDesc: "用于签名令牌的 RS256 密钥。已退役密钥在保留期内仍发布于 JWKS。",
    sysRotate: "轮换密钥",
    sysRotateConfirm: "创建新密钥并退役当前密钥？现有令牌在过期前仍有效。",
    sysRotated: "密钥已轮换。",
    sysRevoke: "吊销",
    sysRevokeConfirm: "立即吊销此密钥？用它签名的所有令牌将立即失效。仅在密钥泄露时使用。",
    sysRevoked: "密钥已吊销。",
    sysKeyId: "密钥 ID",
    sysAlgorithm: "算法",
    sysStatus: "状态",
    sysCreated: "创建时间",
    sysPublishedUntil: "发布至",
    sysActive: "活跃",
    sysRetired: "已退役",
    sysRevokedStatus: "已吊销",
    sysEndpoints: "OpenID Connect 端点",
    sysEndpointsDesc: "提供给依赖方以验证令牌和发现端点。",
    sysIssuer: "签发者",
    sysAudience: "API 受众",
    sysDiscovery: "发现文档",
    sysJwks: "JWKS",
    sysCopy: "复制",
    sysPolicy: "当前安全策略",
    sysPolicyDesc: "来自服务器配置的只读值。",
    sysTokens: "令牌与会话",
    sysAccessToken: "访问令牌有效期",
    sysRefreshIdle: "会话空闲超时",
    sysSessionAbsolute: "会话绝对有效期",
    sysReuseGrace: "刷新重用宽限",
    sysMfaChallenge: "MFA 挑战有效期",
    sysKeyRotation: "自动密钥轮换",
    sysKeyRetention: "退役密钥保留",
    sysProtection: "登录保护",
    sysPasswordPolicy: "密码策略",
    sysLockout: "账户锁定",
    sysRateLimit: "API 速率限制",
    sysAuthRateLimit: "凭证端点限制",
    sysAuditRetention: "审计日志保留",
    sysMinutes: "{{n}} 分钟",
    sysDays: "{{n}} 天",
    sysSeconds: "{{n}} 秒",
    sysPerWindow: "每 IP {{n}} 次 / {{w}} 秒",
    sysLockoutValue: "{{n}} 次失败 → 锁定 {{m}} 分钟",
    sysDisabled: "已禁用",
    sysNoPermission: "需要 SECURITY.READ 权限才能查看此页面。",
    usersTitle: "用户",
    usersSubtitle: "目录中的所有人——管理员和 SSO 账户。",
    usersSearch: "按邮箱或用户名搜索…",
    usersNewSso: "新建 SSO 用户",
    usersColUser: "用户",
    usersColStatus: "状态",
    usersColSecurity: "安全",
    usersColType: "类型",
    usersColLastSignIn: "最近登录",
    usersColJoined: "加入时间",
    usersNever: "从未",
    usersBanned: "已封禁",
    usersInactive: "未激活",
    usersActive: "活跃",
    usersLocked: "已锁定",
    usersUnverified: "未验证",
    usersNoMfa: "无 MFA",
    usersEmpty: "未找到用户。",
    usersStandard: "控制台",
    usersSso: "SSO",
    usersCreateSsoInfo: "创建托管 SSO 账户，之后在用户页面授予应用访问权限。",
    usersRequired: "邮箱、密码、用户名和出生日期为必填。",
    usersCreated: "已创建 SSO 用户。",
    creating: "创建中…",
    create: "创建",
    udBack: "所有用户",
    udCopyEmail: "复制邮箱",
    udCopyId: "复制用户 ID",
    udDisable: "封禁用户",
    udEnable: "恢复访问",
    udRoles: "系统角色",
    udRolesDesc: "角色授予平台范围的权限。",
    udAddRole: "添加角色",
    udSelectRole: "选择角色…",
    udNoRoles: "未分配角色。",
    udRemoveRole: "移除角色",
    udApps: "应用访问",
    udAppsDesc: "该 SSO 账户可登录的应用及角色。",
    udGrant: "授予访问",
    udSelectApp: "选择应用…",
    udSelectAppRole: "选择角色…",
    udNoApps: "尚未授予应用访问。",
    udNoAppsAvailable: "暂无应用。",
    udRevoke: "撤销",
    udSecurity: "安全",
    udSecurityDesc: "多因素认证与已登录设备。",
    udMfaReset: "重置 MFA",
    udMfaResetConfirm: "移除该用户的身份验证器和恢复码？用户将在所有设备退出并需重新注册。",
    udMfaResetDone: "已重置 MFA。",
    udRevokeAll: "全部退出",
    udRevokeAllConfirm: "结束该用户所有会话并撤销所有应用授权？已连接应用将收到后端登出通知。",
    udActiveSessions: "活跃会话",
    udRecentActivity: "近期活动",
    udBanConfirm: "封禁此用户？所有会话和应用授权将立即撤销。",
    udUserId: "用户 ID",
    udBanReason: "封禁原因",
    ssoAccountsTitle: "SSO 账户",
    ssoAccountsSubtitle: "通过单点登录访问已连接应用的托管终端用户账户。",
    appsTitle: "应用",
    appsSubtitle: "将登录委托给此身份提供方的 OAuth 2.0 / OpenID Connect 客户端。",
    appsNew: "新建应用",
    appsSearch: "搜索应用…",
    appsSortName: "排序：名称",
    appsSortOwner: "排序：所有者",
    appsSortBranding: "排序：有图标",
    appsGrid: "网格视图",
    appsList: "列表视图",
    appsEmptyDesc: "注册首个应用以开始使用单点登录。",
    appsOpen: "打开",
    appsDelete: "删除",
    appsDeleteConfirm: "删除“{{name}}”？用户将失去访问权限，其令牌将失效。",
    appsDeleted: "已删除应用。",
    appsColApp: "应用",
    appsColClientId: "Client ID",
    appsColType: "客户端类型",
    appsColOwner: "所有者",
    appsName: "名称",
    appsClientType: "客户端类型",
    appsPublic: "公共",
    appsPublicDesc: "SPA 与移动应用。使用 PKCE，无客户端密钥。",
    appsConfidential: "机密",
    appsConfidentialDesc: "可安全保存客户端密钥的服务端应用。",
    appsRedirects: "允许的重定向 URI",
    appsRedirectsHint: "逗号分隔，精确匹配。除 localhost 外必须为 HTTPS。",
    appsBackchannel: "后端登出 URI",
    appsBackchannelHint: "用户会话结束时接收签名的登出令牌。",
    appsLogo: "图标",
    appsUploadLogo: "上传图片",
    appsLogoHint: "PNG、JPG 或 SVG，最大 2 MB。",
    appsNameRequired: "名称为必填。",
    appsCreated: "已创建应用。",
    appsCredentials: "客户端凭据",
    appsSecretOnce: "请立即复制客户端密钥——它以哈希形式存储，无法再次显示。",
    appsClientSecret: "客户端密钥",
    appsDone: "完成",
    copied: "已复制。",
    adBack: "所有应用",
    adChangeLogo: "更换图标",
    adOverview: "客户端配置",
    adOverviewDesc: "该 OAuth 客户端注册的标识与 URI。",
    adOwner: "所有者",
    adNoRedirects: "未注册重定向 URI——授权请求将被拒绝。",
    adNotConfigured: "未配置",
    adCredentials: "客户端密钥",
    adPublicNote: "公共客户端使用 PKCE 认证，不会获得客户端密钥。",
    adRotate: "轮换密钥",
    adLastRotated: "上次轮换",
    adNeverRotated: "从未轮换",
    adShow: "显示",
    adHide: "隐藏",
    adIntegration: "集成",
    adIntegrationDesc: "授权码 + PKCE (S256)。使用 JWKS 验证令牌，并接收后端登出。",
    adAuthorizeEndpoint: "授权端点",
    adTokenEndpoint: "令牌端点",
    adMembers: "成员",
    adMembersDesc: "允许登录该应用的用户及其角色。",
    adAddMember: "添加成员",
    adRemoveMember: "移除",
    adRemoveMemberConfirm: "从该应用中移除 {{user}}？",
    adMemberRemoved: "已移除成员。",
    adNoMembers: "暂无成员。",
    adNeedRole: "添加成员前请先添加应用角色。",
    adUser: "用户",
    adRole: "角色",
    adRoles: "应用角色",
    adRolesDesc: "该应用成员可用的角色。",
    adAddRole: "添加角色",
    adGlobalRole: "全局角色",
    adNoRoles: "暂无角色。",
    adCapabilities: "您的能力",
    adCapabilitiesDesc: "您在该应用中的功能与权限（来自角色和订阅）。",
    adFeatures: "功能",
    adPermissions: "权限",
    adNone: "无",
    adSelectUser: "选择用户…",
    adSelectRole: "选择角色…",
    adding: "添加中…",
    rlTitle: "角色",
    rlSubtitle: "全局角色聚合权限，可在用户页面分配。",
    rlNew: "新建角色",
    rlEdit: "编辑角色",
    rlSearch: "搜索角色或权限…",
    rlColRole: "角色",
    rlColPermissions: "权限",
    rlColUpdated: "更新",
    rlSystem: "系统",
    rlMore: "+{{n}} 项",
    rlEmpty: "没有匹配的角色。",
    rlName: "名称",
    rlDescription: "描述",
    rlPermissionsPick: "选择权限…",
    rlPermissionsSearch: "按代码或描述搜索…",
    rlSystemLocked: "系统角色不可重命名或删除。",
    rlDeleteConfirm: "删除角色“{{name}}”？持有者将失去其权限。",
    rlNameRequired: "角色名称为必填。",
    rlSaved: "已保存角色。",
    rlDeleted: "已删除角色。",
    saving: "保存中…",
    delete: "删除",
    pmTitle: "权限",
    pmSubtitle: "权限目录。代码格式为 RESOURCE.ACTION，RESOURCE.* 授予所有操作。",
    pmNew: "新建权限",
    pmSearch: "搜索代码、描述或类别…",
    pmColCode: "代码",
    pmColCategory: "类别",
    pmColDescription: "描述",
    pmColScope: "OAuth 范围",
    pmSystem: "系统",
    pmEmpty: "没有匹配的权限。",
    pmCodeHint: "大写 RESOURCE.ACTION，例如 REPORT.EXPORT",
    pmScopeHint: "可选的对应 OAuth 范围。",
    pmCreated: "已创建权限。",
    rpTitle: "角色权限映射",
    rpSubtitle: "选择角色以查看和修改其权限。",
    rpSearchRoles: "筛选角色…",
    rpSelectRole: "在左侧选择角色以管理其权限。",
    rpAssign: "分配",
    rpPickPermission: "添加权限…",
    rpNoPermissions: "该角色未授予任何权限。",
    rpAssigned: "已分配权限。",
    rpRemoved: "已移除权限。",
    rpRemove: "移除权限",
    sbTitle: "订阅",
    sbSubtitle: "为应用成员激活或取消套餐，套餐决定功能与配额。",
    sbApplication: "应用",
    sbMember: "成员",
    sbSelectApp: "选择应用…",
    sbSelectMember: "选择成员…",
    sbPickPrompt: "选择应用和成员以查看其订阅。",
    sbNoMembers: "该应用暂无成员。",
    sbActive: "当前订阅",
    sbNone: "无有效订阅——该成员无法使用此应用。",
    sbActivate: "激活套餐",
    sbCancel: "取消订阅",
    sbCancelConfirm: "取消当前订阅？成员将立即失去套餐功能。",
    sbCancelled: "已取消订阅。",
    sbActivated: "已激活订阅。",
    sbType: "类型",
    sbStarted: "开始",
    sbExpires: "到期",
    sbMembers: "成员",
    sbApiCalls: "API 调用",
    sbFeatures: "功能",
    sbHistory: "历史",
    sbNoHistory: "暂无历史订阅。",
    sbStatus: "状态",
    sbStatusActive: "有效",
    sbStatusCancelled: "已取消",
    sbStatusExpired: "已结束",
    sbPlan: "套餐",
    sbChoosePlan: "选择套餐",
    sbNoPlans: "该应用尚未配置套餐，请在目录中创建。",
    sbPickPlan: "请先选择套餐。",
    sbPerMonth: "/ 月",
    sbUsers: "用户",
    ctTitle: "套餐目录",
    ctSubtitle: "按应用划分的订阅套餐：价格、配额与功能。",
    ctNew: "新建套餐",
    ctEdit: "编辑套餐",
    ctPickApp: "选择应用以管理其套餐。",
    ctSearch: "搜索套餐…",
    ctEmpty: "该应用暂无套餐。",
    ctMonthly: "月价 (VND)",
    ctYearly: "年价 (VND)",
    ctMaxUsers: "最大用户数",
    ctMaxApiCalls: "最大 API 调用",
    ctUnlimitedHint: "留空表示不限。",
    ctFeatures: "包含功能",
    ctOrder: "显示顺序",
    ctIcon: "图标（Material Symbol 名称或图片 URL）",
    ctActive: "套餐可供激活",
    ctDraft: "未启用",
    ctLive: "启用",
    ctSave: "保存套餐",
    ctDeleteConfirm: "删除套餐“{{name}}”？现有订阅保留其权益快照。",
    ctSaved: "已保存套餐。",
    ctDeleted: "已删除套餐。",
    ctNameRequired: "套餐名称为必填。",
    ctSave2: "年付省 {{n}}%",
    ctFeatAnalytics: "分析",
    ctFeatSupport: "优先支持",
    ctFeatSecurity: "高级安全",
    ctFeatDomain: "自定义域名",
    ctPerYear: "/ 年"
  }
};

const LOCALE_KEY = 'app_locale';
const LOCALES: Locale[] = ['en', 'vi', 'zh'];

function initialLocale(): Locale {
  try {
    const stored = localStorage.getItem(LOCALE_KEY) as Locale | null;
    if (stored && LOCALES.includes(stored)) return stored;
  } catch { /* storage unavailable */ }
  const browser = (typeof navigator !== 'undefined' ? navigator.language : 'en').slice(0, 2) as Locale;
  return LOCALES.includes(browser) ? browser : 'en';
}

/**
 * English is the complete base dictionary; other locales are overlays, so a string that has not been
 * translated yet falls back to English instead of rendering empty.
 */
@Injectable({ providedIn: 'root' })
export class TranslateService {
  private localeSignal = signal<Locale>(initialLocale());

  readonly locale = computed(() => this.localeSignal());
  readonly t = computed<AppTranslations>(() => {
    const locale = this.localeSignal();
    return locale === 'en' ? TRANSLATIONS.en : { ...TRANSLATIONS.en, ...TRANSLATIONS[locale] };
  });

  setLocale(locale: Locale) {
    this.localeSignal.set(locale);
    try { localStorage.setItem(LOCALE_KEY, locale); } catch { /* storage unavailable */ }
    document.documentElement.lang = locale;
  }
}
