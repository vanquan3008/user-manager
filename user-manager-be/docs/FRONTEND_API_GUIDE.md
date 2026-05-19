# Frontend API Guide

Tai lieu nay mo ta luong backend theo source that va map truc tiep sang layer goi API ben frontend.

## 1. Nguyen tac chung

- Backend tra envelope:

```json
{
  "success": true,
  "data": {},
  "message": "OK",
  "error": null
}
```

- `accessToken` nam trong body response.
- `refreshToken` duoc backend ghi vao `HttpOnly` cookie, frontend khong doc truc tiep.
- Phan lon endpoint di theo luong:

`Controller -> MediatR Command/Query -> Handler -> Application Service/Aggregate/Repository -> ApiResponse`

- Sau refactor, side effect email verification sau register di qua outbox:

`User.Create -> UserRegisteredEvent -> OutboxMessages -> OutboxProcessorService -> SendVerificationEmailHandler`

Frontend khong doi contract vi outbox chi doi cach backend xu ly side effect.

## 2. Auth flow trong backend

Base route: `/api/auth`

### 2.1 Register

Route:

- `POST /api/auth/register`

Backend flow:

- `AuthController.Register`
- `RegisterUserCommand`
- `RegisterUserCommandHandler`
- `RegistrationService`
- `IIdentityService.RegisterAsync`
- tao `Domain User` bang `User.Create(...)`
- `IUserRepository.AddAsync`
- `IIdentityService.UpdateUserProfileIdAsync`
- `IUnitOfWork.SaveChangesAsync`
- issue refresh-token cookie
- tra `AuthResponse`

Luu y:

- `clientId` la optional
- neu co `clientId`, backend resolve application de scope auth response
- email verification khong gui inline nua, ma gui async qua outbox

FE service:

- `AuthService.register(...)`

Request body:

```json
{
  "email": "user@example.com",
  "password": "P@ssw0rd!",
  "username": "user01",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1999-05-01",
  "clientId": "app_1234567890abcdef"
}
```

### 2.2 Login

Route:

- `POST /api/auth/login`

Backend flow:

- `AuthController.Login`
- `LoginUserCommand`
- `LoginUserCommandHandler`
- `LoginService`
- validate identity user
- build app-scoped auth response neu co `clientId`
- issue refresh-token cookie

FE service:

- `AuthService.login(...)`

### 2.3 Refresh token

Route:

- `POST /api/auth/refresh`

Backend flow:

- `AuthController.RefreshToken`
- lay refresh token tu cookie, body chi la fallback
- `RefreshTokenCommand`
- `RefreshTokenCommandHandler`
- `LoginService.RefreshTokenAsync`
- issue cookie moi

FE service:

- `AuthService.refreshToken()`

### 2.4 Logout

Route:

- `POST /api/auth/logout`

Backend flow:

- `AuthController.Logout`
- neu user dang auth thi goi `LogoutCommand`
- clear refresh-token cookie

FE service:

- `AuthService.logout(...)`

### 2.5 Passwordless / magic link

Routes:

- `POST /api/auth/passwordless/request`
- `POST /api/auth/magic-link/verify`
- `GET /api/auth/magic-link/verify`
- `POST /api/auth/magic-link/resend`

Backend flow:

- request:
  - `RequestPasswordlessLoginCommand`
  - `RequestPasswordlessLoginCommandHandler`
  - `MagicLinkDeliveryService`
- verify:
  - `VerifyMagicLinkCommand`
  - `VerifyMagicLinkCommandHandler`
  - `MagicLinkAuthService`
  - issue refresh-token cookie
- resend:
  - `ResendMagicLinkCommand`
  - `ResendMagicLinkCommandHandler`
  - `MagicLinkDeliveryService`

FE service:

- `AuthService.requestMagicLink(...)`
- `AuthService.verifyMagicLink(...)`
- `AuthService.resendMagicLink(...)`

Request verify:

```json
{
  "token": "magic-link-token",
  "purpose": "PasswordlessLogin",
  "clientId": "app_1234567890abcdef"
}
```

### 2.6 Verify email

Routes:

- `GET /api/auth/verify-email`
- `POST /api/auth/verify-email`

Backend flow:

- token-only path:
  - `VerifyEmailWithTokenCommand`
  - `VerifyEmailWithTokenCommandHandler`
  - `EmailVerificationService`
- legacy body path:
  - neu co `userId` thi di `VerifyEmailCommand`
  - neu khong thi quay lai token-only path

FE service hien dung:

- `AuthService.verifyEmail(token)`

### 2.7 Password reset / change password

Routes:

- `POST /api/auth/password/reset-request`
- `POST /api/auth/password/reset`
- `POST /api/auth/password/change`

Backend flow:

- reset-request:
  - `RequestPasswordResetCommand`
  - `RequestPasswordResetCommandHandler`
  - `PasswordRecoveryService`
- reset:
  - `ResetPasswordCommand`
  - `ResetPasswordCommandHandler`
  - `PasswordRecoveryService`
- change:
  - `ChangePasswordCommand`
  - `ChangePasswordCommandHandler`
  - userId lay tu current principal

FE service:

- `AuthService.requestPasswordReset(...)`
- `AuthService.resetPassword(...)`
- `AuthService.changePassword(...)`

## 3. SSO auth flow trong backend

Routes:

- `POST /api/auth/sso/login`
- `POST /api/auth/sso/authorize`
- `POST /api/auth/sso/consent`
- `POST /api/auth/sso/exchange-token`

### 3.1 SSO login

Backend flow:

- `AuthController.SSOLogin`
- `SSOLoginCommand`
- `SSOLoginCommandHandler`
- `SSOAuthenticationService`

FE service:

- `AuthService.ssoLogin(...)`

### 3.2 SSO authorize

Backend flow:

- `AuthController.SSOAuthorize`
- `SSOAuthorizeCommand`
- handler auth SSO
- neu can consent: tra `requiresConsent = true`
- neu du dieu kien: backend tao `code`, build lai `redirectUri`, tra response

FE service:

- `AuthService.ssoAuthorize(...)`

### 3.3 SSO consent

Backend flow:

- `AuthController.SSOConsent`
- `SSOConsentCommand`
- sau khi approve, backend tao `code` va tra redirect URI day du

FE service:

- `AuthService.ssoConsent(...)`

### 3.4 Exchange SSO session sang app token

Backend flow:

- `AuthController.SSOExchangeToken`
- `SSOExchangeTokenCommand`
- userId duoc gan tu JWT hien tai
- tra `AuthResponse` moi cho target application

FE service:

- `AuthService.ssoExchangeToken(...)`

Luu y:

- Truoc day FE goi thang `api/auth/sso/authorize` va `api/auth/sso/consent` trong component
- Da duoc dua ve `AuthService` de service layer day du hon va de bao tri

## 4. OAuth flow trong backend

Base route: `/api/oauth`

Routes:

- `GET /api/oauth/authorize`
- `POST /api/oauth/consent`
- `POST /api/oauth/token`

Backend flow:

- `AuthorizeOAuthCommand`
- `ConsentOAuthCommand`
- `ExchangeOAuthCodeCommand`

FE service:

- `OauthService.authorize(...)`
- `OauthService.consent(...)`
- `OauthService.exchangeToken(...)`

PKCE:

- FE tu tao `codeVerifier` / `codeChallenge`
- backend validate lai trong flow authorize + token exchange

## 5. Applications flow trong backend

Base route: `/api/applications`

### 5.1 List / detail / create / delete / logo / credentials

Routes:

- `GET /api/applications`
- `GET /api/applications/{applicationId}`
- `POST /api/applications`
- `DELETE /api/applications/{applicationId}`
- `PUT /api/applications/{applicationId}/logo`
- `GET /api/applications/{applicationId}/credentials`
- `POST /api/applications/{applicationId}/credentials/rotate`

Backend flow:

- list:
  - `GetApplicationsQuery`
  - neu current user la admin thi list full
  - nguoc lai list theo membership/application access
- detail:
  - `GetApplicationByIdQuery`
- create:
  - `CreateApplicationCommand`
  - `CreateApplicationCommandHandler`
  - tao aggregate `AppApplication`
  - tao owner membership va role mac dinh
- rotate credential:
  - `RotateApplicationClientSecretCommand`
  - rotate secret trong aggregate/application service

FE service:

- `ApplicationsService.getApplications()`
- `ApplicationsService.getApplication(...)`
- `ApplicationsService.createApplication(...)`
- `ApplicationsService.deleteApplication(...)`
- `ApplicationsService.updateApplicationLogo(...)`
- `ApplicationsService.getApplicationCredentials(...)`
- `ApplicationsService.rotateApplicationClientSecret(...)`

### 5.2 Membership trong application

Routes:

- `GET /api/applications/{applicationId}/users`
- `POST /api/applications/{applicationId}/users`
- `DELETE /api/applications/{applicationId}/users/{userId}`

Backend flow:

- `GetApplicationUsersQuery`
- `AddUserToApplicationCommand`
- `RemoveUserFromApplicationCommand`

FE service:

- `ApplicationsService.getApplicationUsers(...)`
- `ApplicationsService.addUserToApplication(...)`
- `ApplicationsService.removeUserFromApplication(...)`

### 5.3 Application roles va capabilities

Routes:

- `GET /api/applications/{applicationId}/roles`
- `POST /api/applications/{applicationId}/roles`
- `PUT /api/applications/{applicationId}/roles/{roleName}`
- `DELETE /api/applications/{applicationId}/roles/{roleName}`
- `GET /api/applications/{applicationId}/me/capabilities`

Backend flow:

- role endpoints di qua application-role service / handlers
- capabilities endpoint tinh tu:
  - membership
  - effective permissions
  - active subscription plan features

FE service:

- `ApplicationsService.getApplicationRoles(...)`
- `ApplicationsService.createApplicationRole(...)`
- `ApplicationsService.updateApplicationRole(...)`
- `ApplicationsService.deleteApplicationRole(...)`
- `ApplicationsService.getApplicationCapabilities(...)`

Frontend rule:

- `/me/capabilities` la source of truth cho feature flag va permission gating

## 6. Subscription flow trong backend

Base route: `/api/subscriptions`

Routes:

- `GET /api/subscriptions/plans`
- `POST /api/subscriptions/plans`
- `PUT /api/subscriptions/plans/{planId}`
- `DELETE /api/subscriptions/plans/{planId}`
- `POST /api/subscriptions/activate`
- `POST /api/subscriptions/upgrade`
- `POST /api/subscriptions/extend`
- `POST /api/subscriptions/cancel`
- `GET /api/subscriptions/active`
- `GET /api/subscriptions/history`

Backend flow:

- plan catalog:
  - `GetSubscriptionPlansQuery`
  - `CreateSubscriptionPlanCommand`
  - `UpdateSubscriptionPlanCommand`
  - `DeactivateSubscriptionPlanCommand`
- runtime subscription:
  - `ActivateSubscriptionCommand`
  - `UpgradeSubscriptionCommand`
  - `ExtendSubscriptionCommand`
  - `CancelSubscriptionCommand`
  - `GetActiveSubscriptionQuery`
  - `GetSubscriptionHistoryQuery`

DDD note:

- `SubscriptionPlan` tu giu `Features` va `Permissions`
- FE gui feature/permission bang code, backend resolve sang catalog id trong repository

FE service:

- `SubscriptionsService.getPlans(...)`
- `SubscriptionsService.createPlan(...)`
- `SubscriptionsService.updatePlan(...)`
- `SubscriptionsService.deletePlan(...)`
- `SubscriptionsService.activate(...)`
- `SubscriptionsService.upgrade(...)`
- `SubscriptionsService.extend(...)`
- `SubscriptionsService.cancel(...)`
- `SubscriptionsService.getActive(...)`
- `SubscriptionsService.getHistory(...)`

## 7. Roles, permissions, users

### 7.1 Global roles

Routes:

- `GET /api/roles`
- `GET /api/roles/{roleId}`
- `POST /api/roles`
- `PUT /api/roles/{roleId}`
- `DELETE /api/roles/{roleId}`

FE service:

- `RolesService.getRoles()`
- `RolesService.getRole(...)`
- `RolesService.createRole(...)`
- `RolesService.updateRole(...)`
- `RolesService.deleteRole(...)`

### 7.2 Permissions

Routes:

- `GET /api/permissions`
- `GET /api/permissions/{permissionId}`
- `POST /api/permissions`
- `GET /api/permissions/{roleId}/permissions`
- `POST /api/permissions/{roleId}/permissions`
- `POST /api/permissions/{roleId}/permissions/batch`
- `DELETE /api/permissions/{roleId}/permissions/{permissionId}`

FE service:

- `PermissionsService.getPermissions()`
- `PermissionsService.getPermission(...)`
- `PermissionsService.createPermission(...)`
- `PermissionsService.getRolePermissions(...)`
- `PermissionsService.addPermissionToRole(...)`
- `PermissionsService.addPermissionsToRoleBatch(...)`
- `PermissionsService.removePermissionFromRole(...)`

### 7.3 Users va SSO users

Routes:

- `GET /api/users`
- `GET /api/users/{userId}`
- `POST /api/users/{userId}/roles/upgrade`
- `DELETE /api/users/{userId}/roles/{roleName}`
- `POST /api/users/{userId}/ban`
- `POST /api/users/{userId}/unban`
- `POST /api/users/sso`
- `POST /api/users/sso/{ssoUserId}/applications`
- `DELETE /api/users/sso/{ssoUserId}/applications/{applicationId}`
- `GET /api/users/sso/{ssoUserId}/applications`

FE service:

- `UsersService.getUsers(...)`
- `UsersService.getUser(...)`
- `UsersService.addRole(...)`
- `UsersService.removeRole(...)`
- `UsersService.banUser(...)`
- `UsersService.unbanUser(...)`
- `UsersService.createSSOUser(...)`
- `UsersService.grantSSOUserApplication(...)`
- `UsersService.revokeSSOUserApplication(...)`
- `UsersService.getSSOUserApplications(...)`

## 8. Phan FE da bo sung trong dot nay

Da bo sung vao frontend service layer:

- `AuthService.ssoAuthorize(...)`
- `AuthService.ssoConsent(...)`
- `AuthService.ssoExchangeToken(...)`
- `AuthService.resendMagicLink(...)`

Da sua UI:

- `sso-login.component.ts` khong goi URL auth truc tiep nua
- component di qua `AuthService`, dung boundary ro hon va de test/de doi contract hon

## 9. FE implementation rules

1. Dung `AuthService` / `ApplicationsService` / `SubscriptionsService` / `OauthService` lam entry point chinh, tranh goi URL truc tiep trong component.
2. Dung `/api/applications/{applicationId}/me/capabilities` lam source of truth cho UI gating.
3. Khong suy permission tu role name o frontend.
4. `clientId` la integration identifier, khong phai display label.
5. Flow refresh phai dua vao secure cookie do backend issue.
