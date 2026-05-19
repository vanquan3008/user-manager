# Third-Party App Integration Guide

This document describes how an external application integrates with the auth center in the current backend.

## 1. Integration model

Each external app is stored as an `Application`.

Current auth-center behavior:

- the app receives a `clientId`
- the `clientId` is the application's generated API key
- authorization uses PKCE
- the custom per-application OAuth flow uses `/api/oauth/*`
- user access is granted only when the user has:
  - application membership
  - active subscription for that application
  - effective permissions allowed by role and plan

`SuperAdmin` bypasses those checks.

## 2. Important current limitation

Per-application integration currently uses `clientId` plus PKCE.  
There is no public API yet that exposes a per-application client secret for external use.

Because of that:

- use `/api/oauth/authorize`, `/api/oauth/consent`, `/api/oauth/token`
- do not design your external app around `/connect/token` for dynamic `Application` registration
- `/connect/token` belongs to the OpenIddict server configuration, not to the dynamic `Application` client registration model

## 3. Step 1: register the external app

An admin or super admin creates the app:

`POST /api/applications`

```json
{
  "name": "Partner Portal",
  "description": "Third-party partner application",
  "ownerId": "00000000-0000-0000-0000-000000000000",
  "allowedRedirectUris": "https://partner.example.com/auth/callback",
  "backchannelLogoutUri": "https://partner.example.com/api/auth/backchannel-logout"
}
```

Important response fields:

```json
{
  "success": true,
  "data": {
    "id": "application-id",
    "name": "Partner Portal",
    "clientId": "app_1234567890abcdef",
    "description": "Third-party partner application",
    "ownerId": "owner-id",
    "allowedRedirectUris": "https://partner.example.com/auth/callback",
    "backchannelLogoutUri": "https://partner.example.com/api/auth/backchannel-logout"
  }
}
```

Use `data.clientId` in all OAuth requests.

## 4. Step 2: grant user access

The auth center will deny access unless the user is actually allowed into the app.

Required backend state for a normal user:

1. user must exist
2. user must be added to the application
3. user must have an active subscription for that application

Useful endpoints:

- `POST /api/applications/{applicationId}/users`
- `POST /api/subscriptions/activate`
- `POST /api/subscriptions/upgrade`
- `POST /api/subscriptions/extend`

For SSO users:

- `POST /api/users/sso`
- `POST /api/users/sso/{ssoUserId}/applications`

## 5. Step 3: start the authorization code flow

Current backend supports two practical patterns.

### Pattern A: auth-center-hosted login/consent

Recommended when your platform has its own frontend for login and consent screens.

Backend endpoints:

- `GET /api/oauth/authorize`
- `POST /api/oauth/consent`
- `POST /api/oauth/token`

Example authorize request:

```http
GET /api/oauth/authorize?clientId=app_1234567890abcdef&redirectUri=https%3A%2F%2Fpartner.example.com%2Fauth%2Fcallback&scope=openid%20profile%20email&state=opaque-state&codeChallenge=pkce-challenge&codeChallengeMethod=S256
Authorization: Bearer {auth-center-user-token}
```

If consent is needed, call:

```json
POST /api/oauth/consent
{
  "clientId": "app_1234567890abcdef",
  "redirectUri": "https://partner.example.com/auth/callback",
  "scopes": ["openid", "profile", "email"],
  "state": "opaque-state",
  "codeChallenge": "pkce-challenge",
  "codeChallengeMethod": "S256",
  "approve": true
}
```

### Pattern B: credential-based SSO authorization

Mechanically supported, but the external app handles credentials directly.

Endpoints:

- `POST /api/auth/sso/authorize`
- `POST /api/auth/sso/consent`

Example:

```json
{
  "email": "user@example.com",
  "password": "P@ssw0rd!",
  "clientId": "app_1234567890abcdef",
  "redirectUri": "https://partner.example.com/auth/callback",
  "scopes": ["openid", "profile", "email"],
  "state": "opaque-state",
  "codeChallenge": "pkce-challenge",
  "codeChallengeMethod": "S256"
}
```

Use this only when you intentionally want the third-party app to collect credentials.

## 6. Step 4: exchange the authorization code

`POST /api/oauth/token`

```json
{
  "grantType": "authorization_code",
  "clientId": "app_1234567890abcdef",
  "redirectUri": "https://partner.example.com/auth/callback",
  "code": "authorization-code",
  "codeVerifier": "pkce-verifier"
}
```

Response:

```json
{
  "success": true,
  "data": {
    "accessToken": "jwt",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "refreshToken": "refresh-token",
    "scopes": ["openid", "profile", "email"],
    "user": {
      "id": "user-id",
      "email": "user@example.com",
      "userName": "user01",
      "displayName": "user01",
      "roles": ["Admin"]
    }
  }
}
```

## 7. Step 5: refresh the app token

```json
POST /api/oauth/token
{
  "grantType": "refresh_token",
  "clientId": "app_1234567890abcdef",
  "redirectUri": "https://partner.example.com/auth/callback",
  "refreshToken": "refresh-token"
}
```

## 8. Access rules enforced by backend

The external app should expect authorization to fail when:

- the app is inactive
- `redirectUri` does not match the registered allowed URIs
- PKCE verifier does not match
- the user is not a member of the application
- the user has no active subscription for that application
- the user no longer has permission through role-plan intersection

## 9. Recommended onboarding checklist for external apps

1. Create the application in auth center.
2. Save `clientId`.
3. Register exact redirect URIs.
4. Decide whether login is hosted by auth center UI or embedded by the external app.
5. Implement authorization code + PKCE.
6. Handle refresh token rotation.
7. Expect app access to be revoked when membership or subscription is removed.

## 10. Recommended future backend additions

These are not required for the current flow, but would improve third-party integration:

1. return one-time plain client secret on application creation
2. add credential rotation endpoint
3. add hosted browser authorize page on top of the backend flow
4. add external integration audit endpoints
