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
}

const TRANSLATIONS: Record<Locale, AppTranslations> = {
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
    dashBannedBadge: 'Đã cấm'
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
    dashBannedBadge: 'Banned'
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
    dashBannedBadge: '已封禁'
  }
};

@Injectable({ providedIn: 'root' })
export class TranslateService {
  private localeSignal = signal<Locale>('vi');

  readonly locale = computed(() => this.localeSignal());
  readonly t = computed(() => TRANSLATIONS[this.localeSignal()]);

  setLocale(locale: Locale) {
    this.localeSignal.set(locale);
  }
}
