import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { guestGuard } from './core/guards/guest.guard';
import { LayoutComponent } from './layout/layout.component';

export const routes: Routes = [
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./pages/login/credentials/login.component').then(m => m.LoginComponent) },
  { path: 'sso-login', canActivate: [guestGuard], loadComponent: () => import('./pages/login/sso/sso-login.component').then(m => m.SsoLoginComponent) },
  { path: 'auth/callback', loadComponent: () => import('./pages/login/sso/sso-callback.component').then(m => m.SsoCallbackComponent) },
  { path: 'forgot-password', canActivate: [guestGuard], loadComponent: () => import('./pages/login/credentials/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'reset-password', loadComponent: () => import('./pages/login/credentials/reset-password.component').then(m => m.ResetPasswordComponent) },
  { path: 'verify-email', loadComponent: () => import('./pages/login/credentials/verify-email.component').then(m => m.VerifyEmailComponent) },
  { path: 'oauth/authorize', loadComponent: () => import('./pages/login/oauth/oauth-authorize.component').then(m => m.OauthAuthorizeComponent) },
  { path: 'oauth/callback', loadComponent: () => import('./pages/login/oauth/oauth-callback.component').then(m => m.OauthCallbackComponent) },
  {
    path: 'magic-login',
    loadComponent: () =>
      import('./pages/login/magic-link/magic-login-callback.component').then(m => m.MagicLoginCallbackComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'users', loadComponent: () => import('./pages/users/users-list.component').then(m => m.UsersListComponent) },
      { path: 'users/:id', loadComponent: () => import('./pages/users/user-detail.component').then(m => m.UserDetailComponent) },
      { path: 'applications', loadComponent: () => import('./pages/applications/applications.component').then(m => m.ApplicationsComponent) },
      {
        path: 'applications/:id',
        loadComponent: () =>
          import('./pages/applications/application-detail.component').then(m => m.ApplicationDetailComponent)
      },
      {
        path: 'user-admin',
        loadComponent: () => import('./pages/user-admin/user-admin.component').then(m => m.UserAdminComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./pages/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'subscriptions',
        loadComponent: () => import('./pages/billing/subscriptions/subscriptions.component').then(m => m.SubscriptionsComponent)
      },
      {
        path: 'package-catalog',
        loadComponent: () => import('./pages/billing/catalog/package-catalog.component').then(m => m.PackageCatalogComponent)
      },
      {
        path: 'role-permission',
        loadComponent: () => import('./pages/role-permission/role-permission.component').then(m => m.RolePermissionComponent)
      },
      {
        path: 'permissions',
        loadComponent: () => import('./pages/permissions/permissions.component').then(m => m.PermissionsComponent)
      },
      {
        path: 'change-password',
        loadComponent: () => import('./pages/settings/change-password.component').then(m => m.ChangePasswordComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
