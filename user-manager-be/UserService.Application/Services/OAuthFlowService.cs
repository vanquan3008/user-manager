using System.Security.Cryptography;
using System.Text;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.OAuth.Policies;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Services;

public sealed class OAuthFlowService : IOAuthFlowService
{
    private readonly IApplicationRepository _applications;
    private readonly IApplicationOAuthScopeRepository _appScopes;
    private readonly IUserOAuthConsentRepository _consents;
    private readonly IOAuthAuthorizationCodeRepository _codes;
    private readonly IOAuthRefreshTokenRepository _refreshTokens;
    private readonly IPermissionRepository _permissions;
    private readonly IIdentityService _identity;
    private readonly IUserPermissionService _userPermissions;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly ITokenService _tokenService;
    private readonly IHashingService _hashingService;
    private readonly IOAuthRedirectUriPolicy _redirectUriPolicy;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IRequestContext _requestContext;
    private readonly IUserSessionRepository _sessions;
    private readonly IAuditLogger _audit;

    public OAuthFlowService(
        IApplicationRepository applications,
        IApplicationOAuthScopeRepository appScopes,
        IUserOAuthConsentRepository consents,
        IOAuthAuthorizationCodeRepository codes,
        IOAuthRefreshTokenRepository refreshTokens,
        IPermissionRepository permissions,
        IIdentityService identity,
        IUserPermissionService userPermissions,
        IApplicationAccessService applicationAccessService,
        ITokenService tokenService,
        IHashingService hashingService,
        IOAuthRedirectUriPolicy redirectUriPolicy,
        AppSettings appSettings,
        IUnitOfWork unitOfWork,
        IClock clock,
        IRequestContext requestContext,
        IUserSessionRepository sessions,
        IAuditLogger audit)
    {
        _requestContext = requestContext;
        _sessions = sessions;
        _audit = audit;
        _applications = applications;
        _appScopes = appScopes;
        _consents = consents;
        _codes = codes;
        _refreshTokens = refreshTokens;
        _permissions = permissions;
        _identity = identity;
        _userPermissions = userPermissions;
        _applicationAccessService = applicationAccessService;
        _tokenService = tokenService;
        _hashingService = hashingService;
        _redirectUriPolicy = redirectUriPolicy;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<OAuthAuthorizeResponse> AuthorizeAsync(
        Guid userId,
        OAuthAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientId = RequireValue(request.ClientId, "InvalidClientId", "client_id is required.");
        var redirectUri = RequireValue(request.RedirectUri, "InvalidRedirectUri", "redirect_uri is required.");
        if (!string.IsNullOrWhiteSpace(request.ResponseType) && !string.Equals(request.ResponseType.Trim(), "code", StringComparison.Ordinal))
            throw new BusinessRuleValidationException("UnsupportedResponseType", "Only response_type=code is supported.");
        if (request.Nonce is { Length: > 256 })
            throw new BusinessRuleValidationException("InvalidNonce", "nonce is too long.");
        var codeChallengeMethod = RequireValue(
            request.CodeChallengeMethod,
            "InvalidCodeChallengeMethod",
            "Only PKCE S256 is supported.");

        if (!string.Equals(codeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleValidationException("InvalidCodeChallengeMethod", "Only PKCE S256 is supported.");

        var codeChallenge = RequireValue(request.CodeChallenge, "InvalidCodeChallenge", "Missing or invalid code_challenge.");
        if (!OAuthHelpers.IsValidBase64Url(codeChallenge))
            throw new BusinessRuleValidationException("InvalidCodeChallenge", "Missing or invalid code_challenge.");

        var app = await GetActiveApplicationAsync(clientId, cancellationToken);
        await _applicationAccessService.EnsureAccessAsync(userId, app.Id, cancellationToken: cancellationToken);
        ValidateRedirectUri(app, redirectUri);

        var requestedScopes = await ResolveRequestedScopesAsync(app.Id, request.Scopes, cancellationToken);
        var consent = await _consents.GetActiveConsentAsync(userId, app.Id, cancellationToken);
        var alreadyGranted = consent == null
            ? Array.Empty<string>()
            : (await _consents.GetGrantedScopesAsync(consent.Id, cancellationToken)).ToArray();

        var missingScopes = requestedScopes
            .Where(scope => !alreadyGranted.Contains(scope, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (missingScopes.Length > 0 || string.Equals(request.Prompt, "consent", StringComparison.OrdinalIgnoreCase))
        {
            return new OAuthAuthorizeResponse(
                RequiresConsent: true,
                ClientName: app.Name,
                RedirectUri: redirectUri,
                RequestedScopes: requestedScopes,
                AlreadyGrantedScopes: alreadyGranted,
                MissingScopes: missingScopes,
                Code: null,
                State: request.State);
        }

        var code = OAuthHelpers.GenerateCode();
        var authorizationCode = new OAuthAuthorizationCode(
            Guid.NewGuid(),
            SecureTokens.Hash(code),
            userId,
            app.Id,
            redirectUri,
            string.Join(" ", requestedScopes),
            codeChallenge,
            codeChallengeMethod,
            _clock.UtcNow.AddMinutes(2),
            request.Nonce,
            _requestContext.SessionId);

        await _codes.AddAsync(authorizationCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(AuditEventTypes.OAuthCodeIssued, true, new AuditEntry(ActorUserId: userId, ApplicationId: app.Id,
            Details: new { clientId, scopes = requestedScopes }), cancellationToken);

        return new OAuthAuthorizeResponse(
            RequiresConsent: false,
            ClientName: app.Name,
            RedirectUri: redirectUri,
            RequestedScopes: requestedScopes,
            AlreadyGrantedScopes: alreadyGranted,
            MissingScopes: Array.Empty<string>(),
            Code: code,
            State: request.State);
    }

    public async Task<OAuthAuthorizeResponse> ConsentAsync(
        Guid userId,
        OAuthConsentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Approve)
            throw new UnauthorizedAccessException("Consent denied by user.");

        var clientId = RequireValue(request.ClientId, "InvalidClientId", "client_id is required.");
        var redirectUri = RequireValue(request.RedirectUri, "InvalidRedirectUri", "redirect_uri is required.");
        var app = await GetActiveApplicationAsync(clientId, cancellationToken);

        await _applicationAccessService.EnsureAccessAsync(userId, app.Id, cancellationToken: cancellationToken);
        ValidateRedirectUri(app, redirectUri);

        var requestedScopes = await ResolveRequestedScopesAsync(app.Id, request.Scopes, cancellationToken);
        await UpsertConsentAsync(userId, app.Id, requestedScopes, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.OAuthConsentGranted, true, new AuditEntry(ActorUserId: userId, ApplicationId: app.Id,
            Details: new { clientId, scopes = requestedScopes }), cancellationToken);

        return await AuthorizeAsync(
            userId,
            new OAuthAuthorizeRequest(
                ClientId: clientId,
                RedirectUri: redirectUri,
                State: request.State,
                CodeChallenge: request.CodeChallenge,
                CodeChallengeMethod: request.CodeChallengeMethod,
                Prompt: null,
                Scopes: requestedScopes,
                Nonce: request.Nonce),
            cancellationToken);
    }

    public async Task<OAuthTokenResponse> ExchangeTokenAsync(
        OAuthTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientId = (request.ClientId ?? string.Empty).Trim();
        if (clientId.Length == 0)
            throw new OAuthProtocolException(OAuthErrors.InvalidRequest, "client_id is required.");
        var grantType = (request.GrantType ?? string.Empty).Trim();

        var app = await _applications.GetByApiKeyAsync(clientId, cancellationToken);
        if (app == null || !app.IsActive)
            throw new OAuthProtocolException(OAuthErrors.InvalidClient, "Unknown or inactive client.");

        ValidateClientSecret(app, request.ClientSecret);

        try
        {
            if (string.Equals(grantType, "authorization_code", StringComparison.Ordinal))
                return await ExchangeAuthorizationCodeAsync(app, request, cancellationToken);

            if (string.Equals(grantType, "refresh_token", StringComparison.Ordinal))
                return await ExchangeRefreshTokenAsync(app, request, cancellationToken);
        }
        catch (OAuthProtocolException ex)
        {
            await _audit.LogAsync(AuditEventTypes.OAuthTokenFailed, false, new AuditEntry(ApplicationId: app.Id,
                Reason: ex.Error, Details: new { clientId, grantType, ex.ErrorDescription }), cancellationToken);
            throw;
        }

        throw new OAuthProtocolException(OAuthErrors.UnsupportedGrantType, "Supported grant types: authorization_code, refresh_token.");
    }

    public async Task<OAuthUserInfoDto> GetUserInfoAsync(Guid userId, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken = default)
    {
        var user = await _identity.GetUserByIdAsync(userId, cancellationToken)
                   ?? throw new OAuthProtocolException(OAuthErrors.InvalidToken, "User not found.");
        var set = new HashSet<string>(scopes, StringComparer.OrdinalIgnoreCase);
        return new OAuthUserInfoDto(
            user.Id.ToString(),
            set.Contains("email") ? user.Email : null,
            set.Contains("email") ? user.EmailConfirmed : null,
            set.Contains("profile") ? user.UserName : null,
            set.Contains("profile") ? user.UserName : null);
    }

    public async Task RevokeTokenAsync(string clientId, string? clientSecret, string token, CancellationToken cancellationToken = default)
    {
        var app = await _applications.GetByApiKeyAsync((clientId ?? string.Empty).Trim(), cancellationToken);
        if (app == null || !app.IsActive)
            throw new OAuthProtocolException(OAuthErrors.InvalidClient, "Unknown or inactive client.");
        ValidateClientSecret(app, clientSecret);

        if (string.IsNullOrWhiteSpace(token))
            return;

        var entity = await _refreshTokens.GetByHashAsync(SecureTokens.Hash(token.Trim()), cancellationToken);
        // RFC 7009 §2.2: respond identically for unknown tokens and tokens of other clients.
        if (entity == null || entity.ApplicationId != app.Id || entity.IsRevoked)
            return;

        entity.Revoke(_clock.UtcNow, "client_revoked");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(AuditEventTypes.OAuthTokensRevoked, true, new AuditEntry(ActorUserId: entity.UserId, ApplicationId: app.Id,
            Reason: "client_revoked"), cancellationToken);
    }

    private async Task<AppApplication> GetActiveApplicationAsync(string clientId, CancellationToken cancellationToken)
    {
        var app = await _applications.GetByApiKeyAsync(clientId, cancellationToken);
        if (app == null || !app.IsActive)
            throw new BusinessRuleValidationException("OAuthClientInactive", "OAuth client not found or inactive.");

        return app;
    }

    private void ValidateRedirectUri(AppApplication app, string redirectUri)
    {
        var allowedUris = string.Join(",", app.AllowedRedirectUris.Select(uri => (string)uri));
        if (!_redirectUriPolicy.IsAllowed(redirectUri, allowedUris))
            throw new BusinessRuleValidationException("InvalidRedirectUri", "redirect_uri is not allowed.");
    }

    private async Task<string[]> ResolveRequestedScopesAsync(
        Guid applicationId,
        IEnumerable<string>? requestedScopes,
        CancellationToken cancellationToken)
    {
        var requested = (requestedScopes ?? Array.Empty<string>())
            .Select(scope => (scope ?? string.Empty).Trim())
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var allowedScopeCodes = await _appScopes.GetAllowedScopeCodesAsync(applicationId, cancellationToken);
        var allowedDefaultScopes = OAuthScopeNames.FilterDefaultScopes(allowedScopeCodes);
        if (allowedDefaultScopes.Count == 0)
            throw new BusinessRuleValidationException("NoAllowedScopes", "This client has no allowed default scopes configured.");

        if (requested.Length == 0)
            requested = allowedDefaultScopes.ToArray();

        var allowedSet = new HashSet<string>(allowedDefaultScopes, StringComparer.OrdinalIgnoreCase);
        var invalidRequested = requested.Where(scope => !allowedSet.Contains(scope)).ToArray();
        if (invalidRequested.Length > 0)
            throw new BusinessRuleValidationException("InvalidScopes", $"Invalid scope(s) for this client: {string.Join(", ", invalidRequested)}");

        return requested;
    }

    private async Task UpsertConsentAsync(
        Guid userId,
        Guid applicationId,
        IReadOnlyCollection<string> requestedScopes,
        CancellationToken cancellationToken)
    {
        var consent = await _consents.GetActiveConsentAsync(userId, applicationId, cancellationToken);

        var permissionEntities = await _permissions.GetByScopeAsync(requestedScopes, cancellationToken);
        var scopeToId = permissionEntities
            .Where(permission => !string.IsNullOrWhiteSpace(permission.Scope))
            .ToDictionary(permission => permission.Scope!, permission => permission.Id, StringComparer.OrdinalIgnoreCase);

        var missingScopes = requestedScopes.Where(scope => !scopeToId.ContainsKey(scope)).ToArray();
        if (missingScopes.Length > 0)
        {
            var codeEntities = await _permissions.GetByCodeAsync(missingScopes, cancellationToken);
            foreach (var permission in codeEntities)
            {
                scopeToId[permission.Code] = permission.Id;
            }
        }

        var unresolvedScopes = requestedScopes.Where(scope => !scopeToId.ContainsKey(scope)).ToArray();
        if (unresolvedScopes.Length > 0)
            throw new BusinessRuleValidationException("UnknownScopes", $"Unknown scope(s): {string.Join(", ", unresolvedScopes)}");

        var permissionIds = requestedScopes.Select(scope => scopeToId[scope]).ToArray();

        if (consent == null)
        {
            consent = new UserOAuthConsent(Guid.NewGuid(), userId, applicationId);
            consent.ReplaceScopes(permissionIds);
            await _consents.AddAsync(consent, cancellationToken);
        }
        else
        {
            if (!consent.IsActive)
                consent.Activate();

            consent.ReplaceScopes(permissionIds);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<OAuthTokenResponse> ExchangeAuthorizationCodeAsync(
        AppApplication app,
        OAuthTokenRequest request,
        CancellationToken cancellationToken)
    {
        var codeValue = (request.Code ?? string.Empty).Trim();
        if (codeValue.Length == 0)
            throw new OAuthProtocolException(OAuthErrors.InvalidRequest, "code is required.");

        var entity = await _codes.GetByCodeHashAsync(SecureTokens.Hash(codeValue), cancellationToken);
        if (entity == null || entity.ApplicationId != app.Id)
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Invalid authorization code.");

        if (entity.IsConsumed)
        {
            await HandleCodeReplayAsync(entity, cancellationToken);
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Authorization code has already been used.");
        }

        if (entity.IsExpired(_clock.UtcNow))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Authorization code expired.");

        var redirectUri = (request.RedirectUri ?? string.Empty).Trim();
        if (!string.Equals(entity.RedirectUri, redirectUri, StringComparison.Ordinal))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "redirect_uri does not match the authorization request.");

        if (!string.Equals(entity.CodeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Unsupported PKCE method.");

        var verifier = (request.CodeVerifier ?? string.Empty).Trim();
        // RFC 7636 §4.1: 43-128 characters from the unreserved set.
        if (verifier.Length is < 43 or > 128 || !verifier.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '.' or '_' or '~'))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Invalid code_verifier.");
        if (!SecureTokens.FixedTimeEquals(ComputeS256Challenge(verifier), entity.CodeChallenge))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Invalid code_verifier.");

        entity.Consume(_clock.UtcNow);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Two concurrent redemptions of the same code: only one may win.
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Authorization code has already been used.");
        }

        return await IssueTokenResponseAsync(app, entity.UserId, entity.Scopes, entity.SessionId, entity.Nonce, includeIdToken: true, cancellationToken);
    }

    /// <summary>RFC 6749 §4.1.2: on code reuse, revoke tokens previously issued from that code.</summary>
    private async Task HandleCodeReplayAsync(OAuthAuthorizationCode entity, CancellationToken cancellationToken)
    {
        var revoked = await _refreshTokens.RevokeAllForUserClientAsync(entity.UserId, entity.ApplicationId, "code_replay", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(AuditEventTypes.OAuthCodeReplay, false, new AuditEntry(
            ActorUserId: entity.UserId, ApplicationId: entity.ApplicationId,
            Reason: "Authorization code presented more than once.", Details: new { revokedTokens = revoked }), cancellationToken);
    }

    private async Task<OAuthTokenResponse> ExchangeRefreshTokenAsync(
        AppApplication app,
        OAuthTokenRequest request,
        CancellationToken cancellationToken)
    {
        var refreshToken = (request.RefreshToken ?? string.Empty).Trim();
        if (refreshToken.Length == 0)
            throw new OAuthProtocolException(OAuthErrors.InvalidRequest, "refresh_token is required.");

        var entity = await _refreshTokens.GetByHashAsync(SecureTokens.Hash(refreshToken), cancellationToken);
        if (entity == null || entity.ApplicationId != app.Id)
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Invalid refresh token.");

        if (entity.IsRevoked)
        {
            if (entity.WasRotated)
            {
                // Replay of a rotated token: assume theft and revoke the entire token family for this client.
                var revoked = await _refreshTokens.RevokeAllForUserClientAsync(entity.UserId, app.Id, "reuse_detected", cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _audit.LogAsync(AuditEventTypes.OAuthRefreshReuse, false, new AuditEntry(
                    ActorUserId: entity.UserId, ApplicationId: app.Id,
                    Reason: "Rotated refresh token was replayed.", Details: new { revokedTokens = revoked }), cancellationToken);
            }
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Refresh token has been revoked.");
        }

        if (entity.IsExpired(_clock.UtcNow))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Refresh token expired.");

        if (entity.SessionId.HasValue)
        {
            var session = await _sessions.GetByIdAsync(entity.SessionId.Value, cancellationToken);
            if (session is null || !session.IsActive(_clock.UtcNow))
                throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "The single sign-on session has ended.");
        }

        entity.Revoke(_clock.UtcNow, OAuthRefreshToken.RevokedByRotation);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "Refresh token has already been used.");
        }

        return await IssueTokenResponseAsync(app, entity.UserId, entity.Scopes, entity.SessionId, nonce: null, includeIdToken: false, cancellationToken);
    }

    private async Task<OAuthTokenResponse> IssueTokenResponseAsync(
        AppApplication app,
        Guid userId,
        string scopes,
        Guid? sessionId,
        string? nonce,
        bool includeIdToken,
        CancellationToken cancellationToken)
    {
        var user = await _identity.GetUserByIdAsync(userId, cancellationToken);
        if (user == null || !await _identity.IsSignInAllowedAsync(userId, cancellationToken))
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "The user account is not available.");

        var roles = await _identity.GetUserRolesAsync(userId, cancellationToken);
        var hasAccess = await _applicationAccessService.HasAccessAsync(userId, app.Id, roles, cancellationToken);
        if (!hasAccess)
            throw new OAuthProtocolException(OAuthErrors.InvalidGrant, "User does not have active access to this application.");

        var permissions = await _userPermissions.GetPermissionCodesForUserAsync(userId, app.Id, cancellationToken);
        var grantedScopes = scopes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var oauthScopes = OAuthScopeNames.FilterDefaultScopes(grantedScopes);
        var clientId = app.ApiKey.Value;

        var accessToken = _tokenService.GenerateAccessToken(
            new AccessTokenRequest(
                userId,
                user.Email,
                user.UserName,
                roles,
                permissions,
                app.Id,
                Scope: null,
                SSOAppIds: null,
                OAuthScopes: oauthScopes,
                SessionId: sessionId,
                ClientId: clientId));

        string? idToken = null;
        if (includeIdToken && grantedScopes.Contains("openid", StringComparer.OrdinalIgnoreCase))
        {
            var session = sessionId.HasValue ? await _sessions.GetByIdAsync(sessionId.Value, cancellationToken) : null;
            idToken = _tokenService.GenerateIdToken(new IdTokenRequest(
                userId,
                clientId,
                nonce,
                sessionId,
                session?.CreatedAt ?? _clock.UtcNow,
                grantedScopes,
                user.Email,
                user.EmailConfirmed,
                user.UserName,
                user.UserName,
                session?.AuthenticationMethod));
        }

        var refreshToken = _tokenService.GenerateRefreshToken();
        await _refreshTokens.AddAsync(
            OAuthRefreshToken.Create(
                Guid.NewGuid(),
                userId,
                app.Id,
                TokenValue.Create(SecureTokens.Hash(refreshToken)),
                string.Join(" ", grantedScopes),
                _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
                sessionId),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(AuditEventTypes.OAuthTokenIssued, true, new AuditEntry(ActorUserId: userId, ApplicationId: app.Id, SessionId: sessionId,
            Details: new { clientId, scopes = grantedScopes, idToken = idToken != null }), cancellationToken);

        return new OAuthTokenResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: (int)TimeSpan.FromMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes).TotalSeconds,
            RefreshToken: refreshToken,
            Scopes: grantedScopes,
            User: new OAuthUserDto(
                user.Id,
                user.Email,
                user.UserName,
                user.UserName,
                roles),
            IdToken: idToken);
    }

    private void ValidateClientSecret(AppApplication app, string? clientSecret)
    {
        if (!ApplicationClientTypes.UsesClientSecret(app.ClientType))
            return;

        var providedSecret = (clientSecret ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(providedSecret) || !_hashingService.Verify(providedSecret, app.ApiSecretHash.Value))
            throw new OAuthProtocolException(OAuthErrors.InvalidClient, "Client authentication failed.");
    }

    private static string RequireValue(string? value, string code, string message)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new BusinessRuleValidationException(code, message);

        return trimmed;
    }

    private static string ComputeS256Challenge(string verifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return OAuthHelpers.Base64UrlEncode(bytes);
    }

}
