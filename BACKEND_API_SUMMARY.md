# Backend and Frontend Flow Summary

## 1. Kien truc hien tai

Backend dang di theo huong `Clean Architecture + CQRS + DDD-lite`:

- `UserService.Api`: HTTP endpoint, authN/authZ, middleware, request mapping.
- `UserService.Application`: use case, MediatR handler, validator, application policy, orchestration.
- `UserService.Domain`: aggregate, entity, value object, domain event, repository contract.
- `UserService.Infrastructure`: EF Core, Identity, repository implementation, email, storage, seeding.

Luong request chuan:

`Controller -> MediatR Command/Query -> Validator -> Handler -> Aggregate/Repository -> UnitOfWork -> DTO response`

## 2. Nhung gi da lam trong dot nay

### 2.1 Tach `AuthService` theo use case

Runtime khong con di qua mot service auth tong hop nua. Cac flow auth da duoc tach thanh service nho hon:

- `RegistrationService`
- `LoginService`
- `SsoAuthenticationService`
- `MagicLinkAuthService`
- `MagicLinkDeliveryService`
- `PasswordRecoveryService`
- `EmailVerificationService`
- `AuthResponseFactory` cho phan build token/user response dung chung

Handler hien tai map ro hon:

- `RegisterUserCommandHandler -> IRegistrationService`
- `LoginUserCommandHandler -> ILoginService`
- `RefreshTokenCommandHandler -> ILoginService`
- `SsoLoginCommandHandler -> ISsoAuthenticationService`
- `VerifyMagicLinkCommandHandler -> IMagicLinkAuthService`
- `RequestPasswordlessLoginCommandHandler -> IMagicLinkDeliveryService`
- `ResendMagicLinkCommandHandler -> IMagicLinkDeliveryService`
- `RequestPasswordResetCommandHandler -> IPasswordRecoveryService`
- `VerifyEmailCommandHandler -> IEmailVerificationService`
- `VerifyEmailWithTokenCommandHandler -> IEmailVerificationService`

`AuthService` cu va cac interface auth tong hop cu da duoc xoa khoi codebase vi khong con call-site production.

### 2.2 Day invariant vao aggregate `SubscriptionPlan`

`SubscriptionPlan` khong con de repository replace feature/permission truc tiep nua.

Aggregate nay gio so huu:

- `Features`
- `Permissions`
- `ReplaceFeatureAssignments(...)`
- `ReplacePermissionAssignments(...)`

Command side da duoc doi:

- `CreateSubscriptionPlanCommandHandler`
- `UpdateSubscriptionPlanCommandHandler`

Repository gio chi con:

- resolve feature code -> feature id
- resolve permission code -> permission id
- load aggregate kem catalog data khi can update

Nghia la rule ghi du lieu da duoc dua tro lai aggregate thay vi nam o repository.

### 2.3 Chuan hoa domain policy / application policy

`IApplicationAccessService` da co them `EvaluateAsync(...)` va record:

- `ApplicationAccessEvaluation`

Policy nay tach ro 3 lop dieu kien:

- `HasSystemRoleAccess`
- `HasActiveMembership`
- `HasActiveSubscription`

`HasAccessAsync(...)` bay gio chi la wrapper tren evaluation result, giup policy ro va de mo rong hon.

### 2.4 Architecture tests

Da them test boundary tai:

[LayerDependencyTests.cs](D:/Web/user-manager/user-manager-be/UserService.Tests/Architecture/LayerDependencyTests.cs:1)

Hai rule dang duoc khoa:

- `Domain` khong duoc reference `Microsoft.AspNetCore*` hoac `Microsoft.EntityFrameworkCore*`
- `Application` khong duoc reference `Microsoft.EntityFrameworkCore*` hoac `UserService.Infrastructure`

Luu y:

- Toi khong khoa rule `Api -> Infrastructure` vi trong solution nay `Api` dang la composition root, nen direct reference sang infrastructure de wire DI la chap nhan duoc. Neu muon cam hoan toan, can doi cach bootstrap.

### 2.5 Module hoa them theo bounded context

Tang DI da duoc tach ro hon theo module:

- `Identity`
- `Applications`
- `Authorization`
- `Subscriptions`
- `Outbox`

Dieu nay chua phai tach thanh assembly rieng, nhung boundary dang ro hon o composition level:

- `UserService.Application/DependencyInjection/*ModuleServiceCollectionExtensions.cs`
- `UserService.Infrastructure/DependencyInjection/*InfrastructureServiceCollectionExtensions.cs`

### 2.6 Outbox / domain-event processing

Da implement buoc dau cho outbox theo domain event that su:

- `UserDbContext.SaveChangesAsync()` extract domain event tu aggregate
- domain event duoc serialize vao bang `OutboxMessages` trong cung transaction
- `OutboxProcessorService` poll outbox, deserialize event, publish lai qua `MediatR`
- co config:
  - `Enabled`
  - `BatchSize`
  - `PollIntervalSeconds`
  - `MaxRetryCount`

Luong dang dung that:

- `User.Create(...)` phat `UserRegisteredEvent`
- `SendVerificationEmailHandler` duoc goi async thong qua outbox processor

Tac dong len FE:

- khong doi API contract
- email verification sau register tro thanh side effect async sau commit, dung hon voi huong event-driven

### 2.7 Chuan hoa migration naming

Migration class da duoc doi ten:

- `refactordatabase` -> `RefactorDatabase`

Ket qua:

- `dotnet build UserService.sln` hien tai pass `0 warning`, `0 error`

### 2.8 Dead code / shape cleanup

Da don phan khong con duoc dung:

- xoa `RefreshTokenRequest`
- xoa `MagicLinkVerifiedEvent`
- xoa `PagedResult<T>`
- xoa `OAuthTokenResponseSource`
- bo API cache khong co call-site that
- xoa `AuthService` cu va cac interface auth tong hop cu
- xoa `IDomainEventDispatcher` / `DomainEventDispatcher` cu
- xoa logging-only event handler khong co side effect nghiep vu

## 3. Trang thai doi chieu voi uu tien 1-7

1. Tach `AuthService`: `Da lam`
   Runtime auth da tach thanh service nho theo use case.

2. Day invariant vao aggregate: `Da lam buoc chinh`
   `SubscriptionPlan` da tu quan ly feature/permission assignment.

3. Tach bounded context theo module: `Da lam them mot buoc`
   DI cua `Applications`, `Authorization`, `Subscriptions` da duoc tach thanh module rieng. Chua tach thanh project/assembly rieng.

4. Chuan hoa domain policy/domain service: `Da lam`
   `ApplicationAccessService` da co evaluation result ro tung rule.

5. Them outbox: `Da implement buoc dau`
   Da co outbox table + processor + retry gioi han + publish lai qua MediatR cho domain event sau commit.

6. Them architecture tests: `Da lam`
   Da them boundary test cho `Domain` va `Application`.

7. Chuan hoa migration naming: `Da lam`
   Warning migration lowercase da het.

## 4. Luong backend chinh sau refactor

### 4.1 Auth / Account

- `POST /api/auth/register`
  `RegisterRequest -> RegisterUserCommand -> RegisterUserCommandHandler -> RegistrationService`

- `POST /api/auth/login`
  `LoginRequest -> LoginUserCommand -> LoginUserCommandHandler -> LoginService`

- `POST /api/auth/refresh`
  `RefreshTokenCommand -> RefreshTokenCommandHandler -> LoginService`

- `POST /api/auth/sso/login`
  `SsoLoginCommand -> SsoLoginCommandHandler -> SsoAuthenticationService`

- `POST /api/auth/passwordless/request`
  `RequestPasswordlessLoginCommand -> RequestPasswordlessLoginCommandHandler -> MagicLinkDeliveryService`

- `POST /api/auth/magic-link/verify`
  `VerifyMagicLinkCommand -> VerifyMagicLinkCommandHandler -> MagicLinkAuthService`

- `POST /api/auth/password/reset-request`
  `RequestPasswordResetCommand -> RequestPasswordResetCommandHandler -> PasswordRecoveryService`

- `POST /api/auth/verify-email` va `GET /api/auth/verify-email`
  di vao `EmailVerificationService`

### 4.2 Application

- `POST /api/applications`
  tao `AppApplication`, seed role mac dinh, tao membership owner

- `POST /api/applications/{applicationId}/users`
  kiem tra duplicate membership, resolve role, tao `ApplicationMembership`

- `PUT /api/applications/{applicationId}/logo`
  update logo qua aggregate `AppApplication`

- `POST /api/applications/{applicationId}/credentials/rotate`
  xoay secret cho confidential client

- `GET /api/applications/{applicationId}/me/capabilities`
  vao `ICapabilityService` va policy access/permission

### 4.3 Subscription / Plan

- `POST /api/subscriptions/plans`
  tao `SubscriptionPlan`, sau do aggregate tu sync feature/permission assignment

- `PUT /api/subscriptions/plans/{planId}`
  load `SubscriptionPlan` kem catalog data, update aggregate, sync assignment ngay trong domain object

- `POST /api/subscriptions/activate|upgrade|extend|cancel`
  van di qua handler CQRS va `Subscription` aggregate

## 5. Doi chieu frontend voi backend

Frontend dang dung backend theo nhung luong chinh sau.

### 5.1 Auth flow tren FE

Route:

- `/login`
- `/forgot-password`
- `/reset-password`
- `/verify-email`
- `/magic-login`
- `/sso-login`
- `/oauth/authorize`
- `/auth/callback`

Service FE:

- [auth.service.ts](D:/Web/user-manager/user-manager-fe/src/app/core/services/auth.service.ts:1)
- [oauth.service.ts](D:/Web/user-manager/user-manager-fe/src/app/core/services/oauth.service.ts:1)

Map backend:

- `AuthService.register()` -> `POST /api/auth/register`
- `AuthService.login()` -> `POST /api/auth/login`
- `AuthService.requestMagicLink()` -> `POST /api/auth/passwordless/request`
- `AuthService.verifyMagicLink()` -> `POST /api/auth/magic-link/verify`
- `AuthService.requestPasswordReset()` -> `POST /api/auth/password/reset-request`
- `AuthService.resetPassword()` -> `POST /api/auth/password/reset`
- `AuthService.verifyEmail()` -> `GET /api/auth/verify-email`
- `AuthService.refreshToken()` -> `POST /api/auth/refresh`
- `AuthService.logout()` -> `POST /api/auth/logout`
- `OauthService.authorize()` -> `GET /api/oauth/authorize`
- `OauthService.consent()` -> `POST /api/oauth/consent`
- `OauthService.exchangeToken()` -> `POST /api/oauth/token`

### 5.2 Application flow tren FE

Route:

- `/applications`
- `/applications/:id`

Service FE:

- [applications.service.ts](D:/Web/user-manager/user-manager-fe/src/app/core/services/applications.service.ts:1)

Page chi tiet application hien tai dang goi cung luc:

- detail app
- credentials
- capabilities
- application roles
- application users

Tuc la page detail dang map ro vao cac endpoint:

- `GET /api/applications/{id}`
- `GET /api/applications/{id}/credentials`
- `GET /api/applications/{id}/me/capabilities`
- `GET /api/applications/{id}/roles`
- `GET /api/applications/{id}/users`

### 5.3 Billing / plan catalog tren FE

Route:

- `/package-catalog`
- `/subscriptions`

Service FE:

- [subscriptions.service.ts](D:/Web/user-manager/user-manager-fe/src/app/core/services/subscriptions.service.ts:1)

Catalog page:

- [package-catalog.component.ts](D:/Web/user-manager/user-manager-fe/src/app/pages/billing/catalog/package-catalog.component.ts:1)

Luong catalog:

- load application list
- chon application
- `GET /api/subscriptions/plans?applicationId=...`
- create/update/delete plan qua:
  - `POST /api/subscriptions/plans`
  - `PUT /api/subscriptions/plans/{planId}`
  - `DELETE /api/subscriptions/plans/{planId}`

Luong subscription runtime:

- `GET /api/subscriptions/active`
- `GET /api/subscriptions/history`
- `POST /api/subscriptions/activate`
- `POST /api/subscriptions/cancel`

### 5.4 Mismatch FE/BE da duoc chinh

Model FE `SsoApplication` truoc day bat buoc:

- `role`
- `canRead`
- `canWrite`
- `canDelete`
- `canManageUsers`
- `canManageSubscription`

Nhung backend auth response hien tai chi tra ve:

- `id`
- `name`
- `description`

Vi vay FE da duoc doi cho cac field capability/role thanh optional tai:

- [auth.model.ts](D:/Web/user-manager/user-manager-fe/src/app/core/models/auth.model.ts:1)

Dieu nay dung hon voi contract hien tai va tranh assumption sai tu auth response.

## 6. Danh gia DDD hien tai

Neu danh gia thang:

- ve layering: `tot`
- ve CQRS/use-case separation: `tot hon truoc ro ret`
- ve aggregate boundary: `da dung huong nhung chua full`
- ve bounded context: `moi tach tung phan, chua module hoa hoan chinh`
- ve event architecture/outbox: `chua co`

Ket luan:

Backend hien tai da tien them mot buoc ro ret ve DDD-thuc-dung:

- auth flow da tach theo use case
- subscription-plan aggregate da giu assignment invariant
- application/authorization/subscription boundary da ro hon o tang DI
- outbox da co cho domain event that su

No van chua phai DDD strict hoan chinh. Buoc tiep theo hop ly nhat la:

1. tiep tuc module hoa folder/namespace/service theo bounded context thay vi moi tach o DI
2. tach integration-event contract rieng neu sau nay co webhook, audit, analytics, provisioning
