# Integrating an application (relying party)

This guide covers connecting your own application to Enterprise SSO with the OAuth 2.0 authorization-code flow,
PKCE and OpenID Connect. Endpoint URLs come from discovery, so read them there instead of hard-coding them:

```
GET {API}/.well-known/openid-configuration
```

## 1. Register the application

In the console go to **Applications → New application**:

- **Client type**: *public* (SPA / mobile, no secret) or *confidential* (server-side, with a secret that is shown once).
- **Redirect URIs**: exact matches only. Use HTTPS outside local development.
- **Back-channel logout URI** (optional): this endpoint receives logout tokens when the user's SSO session ends.
- Add the users who may sign in as **members**, each with an application role and an active subscription.
  Sign-in is refused for anyone who is not a member.

## 2. Send the user to the authorization endpoint

```
GET {authorization_endpoint}
  ?response_type=code
  &client_id=<client id>
  &redirect_uri=<registered redirect URI>
  &scope=openid profile email
  &state=<random, bound to the browser session>
  &nonce=<random>
  &code_challenge=<BASE64URL(SHA256(code_verifier))>
  &code_challenge_method=S256
```

PKCE with `S256` is mandatory, and `response_type=code` is the only supported response type. The user signs in on the
hosted page (with MFA if it is enabled), approves the requested scopes once, and is redirected to
`redirect_uri?code=…&state=…`. Errors come back as `redirect_uri?error=…&state=…` (for example `access_denied`).

## 3. Exchange the code

```bash
curl -s {token_endpoint} \
  -u "<client id>:<client secret>" \        # confidential clients (or client_secret in the form body)
  -d grant_type=authorization_code \
  -d client_id=<client id> \
  -d redirect_uri=<same redirect URI> \
  -d code=<code> \
  -d code_verifier=<code_verifier>
```

```json
{ "access_token": "…", "token_type": "Bearer", "expires_in": 900,
  "refresh_token": "…", "scope": "openid profile email", "id_token": "…" }
```

Codes expire after 2 minutes and can be used once. Presenting a used code again revokes the tokens issued from it.

## 4. Validate the ID token

- Verify the RS256 signature with a key from `jwks_uri`, chosen by the token's `kid` header. Cache the JWKS and
  refresh it when an unknown `kid` appears, because keys rotate.
- `iss` must equal the discovery `issuer`, and `aud` must equal your client id.
- `exp` must be in the future, and `nonce` must equal the value you sent.
- Store `sid`: back-channel logout uses it to identify the session.

## 5. Use and refresh tokens

- Call `userinfo_endpoint` with `Authorization: Bearer <access_token>`. It returns claims for the granted scopes.
- Refresh with `grant_type=refresh_token&refresh_token=…&client_id=…` plus client authentication. Each refresh
  returns a **new** refresh token, so store it and discard the old one. Presenting a rotated refresh token again is
  treated as theft and revokes the whole token family.
- Revoke on sign-out: `POST {revocation_endpoint}` with `token=…&client_id=…` (RFC 7009).

## 6. Handle back-channel logout

When the user's SSO session ends (logout, password reset, an admin revoking it, a ban), the API POSTs the following
to your back-channel URI:

```
Content-Type: application/x-www-form-urlencoded

logout_token=<JWT>
```

Validate the JWT the same way as an ID token (signature, `iss`, `aud`). Also check that its `typ` header is
`logout+jwt`, that the `events` claim contains `http://schemas.openid.net/event/backchannel-logout`, and that it has
no `nonce`. Then end the local sessions that match its `sid` (or `sub`). Return `200` quickly: the identity provider
waits at most 5 seconds and does not retry.

Your refresh tokens for that session are already revoked by the time the request arrives.
