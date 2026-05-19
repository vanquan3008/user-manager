using System.Security.Cryptography;
using System.Text;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.OAuth.Policies;
using UserService.Application.Interfaces.Services;
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
        IClock clock)
    {
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
            code,
            userId,
            app.Id,
            redirectUri,
            string.Join(" ", requestedScopes),
            codeChallenge,
            codeChallengeMethod,
            _clock.UtcNow.AddMinutes(2));

        await _codes.AddAsync(authorizationCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        return await AuthorizeAsync(
            userId,
            new OAuthAuthorizeRequest(
                ClientId: clientId,
                RedirectUri: redirectUri,
                State: request.State,
                CodeChallenge: request.CodeChallenge,
                CodeChallengeMethod: request.CodeChallengeMethod,
                Prompt: null,
                Scopes: requestedScopes),
            cancellationToken);
    }

    public async Task<OAuthTokenResponse> ExchangeTokenAsync(
        OAuthTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientId = RequireValue(request.ClientId, "InvalidClientId", "client_id is required.");
        var grantType = RequireValue(request.GrantType, "GrantTypeUnsupported", "Unsupported grant_type.");
        var app = await GetActiveApplicationAsync(clientId, cancellationToken);

        ValidateClientSecret(app, request);

        if (string.Equals(grantType, "authorization_code", StringComparison.OrdinalIgnoreCase))
            return await ExchangeAuthorizationCodeAsync(app.Id, request, cancellationToken);

        if (string.Equals(grantType, "refresh_token", StringComparison.OrdinalIgnoreCase))
            return await ExchangeRefreshTokenAsync(app.Id, request, cancellationToken);

        throw new BusinessRuleValidationException("GrantTypeUnsupported", "Unsupported grant_type.");
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
        Guid appId,
        OAuthTokenRequest request,
        CancellationToken cancellationToken)
    {
        var codeValue = RequireValue(request.Code, "InvalidAuthorizationCode", "Invalid authorization code.");
        var entity = await _codes.GetByCodeAsync(codeValue, cancellationToken);
        if (entity == null)
            throw new UnauthorizedAccessException("Invalid authorization code.");

        if (entity.ApplicationId != appId)
            throw new UnauthorizedAccessException("Authorization code does not belong to this client.");

        var redirectUri = (request.RedirectUri ?? string.Empty).Trim();
        if (!string.Equals(entity.RedirectUri, redirectUri, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("redirect_uri mismatch.");

        if (entity.IsConsumed)
            throw new UnauthorizedAccessException("Authorization code already consumed.");

        if (entity.IsExpired(_clock.UtcNow))
            throw new UnauthorizedAccessException("Authorization code expired.");

        if (!string.Equals(entity.CodeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleValidationException("PkceMethodUnsupported", "Unsupported PKCE method.");

        var verifier = RequireValue(request.CodeVerifier, "InvalidCodeVerifier", "Invalid code_verifier.");
        var expectedChallenge = ComputeS256Challenge(verifier);
        if (!string.Equals(expectedChallenge, entity.CodeChallenge, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid code_verifier.");

        entity.Consume(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokenResponseAsync(appId, entity.UserId, entity.Scopes, revokeExistingRefreshTokens: true, cancellationToken);
    }

    private async Task<OAuthTokenResponse> ExchangeRefreshTokenAsync(
        Guid appId,
        OAuthTokenRequest request,
        CancellationToken cancellationToken)
    {
        var refreshToken = RequireValue(request.RefreshToken, "RefreshTokenRequired", "Missing refresh_token.");
        var tokenHash = ComputeSha256Hex(refreshToken);
        var entity = await _refreshTokens.GetActiveByHashAsync(tokenHash, cancellationToken);
        if (entity == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (entity.ApplicationId != appId)
            throw new UnauthorizedAccessException("Refresh token does not belong to this client.");

        if (entity.IsExpired(_clock.UtcNow))
            throw new UnauthorizedAccessException("Refresh token expired.");

        if (entity.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token revoked.");

        entity.Revoke(_clock.UtcNow);
        return await IssueTokenResponseAsync(appId, entity.UserId, entity.Scopes, revokeExistingRefreshTokens: false, cancellationToken);
    }

    private async Task<OAuthTokenResponse> IssueTokenResponseAsync(
        Guid appId,
        Guid userId,
        string scopes,
        bool revokeExistingRefreshTokens,
        CancellationToken cancellationToken)
    {
        var user = await _identity.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new BusinessRuleValidationException("UserNotFound", "User not found.");

        var roles = await _identity.GetUserRolesAsync(userId, cancellationToken);
        var hasAccess = await _applicationAccessService.HasAccessAsync(userId, appId, roles, cancellationToken);
        if (!hasAccess)
            throw new BusinessRuleValidationException("AppAccessDenied", "User does not have active access to this application.");

        var permissions = await _userPermissions.GetPermissionCodesForUserAsync(userId, appId, cancellationToken);
        var oauthScopes = OAuthScopeNames.FilterDefaultScopes(
            scopes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var accessToken = _tokenService.GenerateAccessToken(
            new AccessTokenRequest(
                userId,
                user.Email,
                user.UserName,
                roles,
                permissions,
                appId,
                Scope: null,
                SSOAppIds: null,
                OAuthScopes: oauthScopes));

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = ComputeSha256Hex(refreshToken);

        if (revokeExistingRefreshTokens)
            await _refreshTokens.RevokeAllForUserClientAsync(userId, appId, cancellationToken);

        await _refreshTokens.AddAsync(
            OAuthRefreshToken.Create(
                Guid.NewGuid(),
                userId,
                appId,
                TokenValue.Create(refreshTokenHash),
                string.Join(" ", oauthScopes),
                _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OAuthTokenResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: (int)TimeSpan.FromMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes).TotalSeconds,
            RefreshToken: refreshToken,
            Scopes: oauthScopes.ToArray(),
            User: new OAuthUserDto(
                user.Id,
                user.Email,
                user.UserName,
                user.UserName,
                roles));
    }

    private void ValidateClientSecret(AppApplication app, OAuthTokenRequest request)
    {
        if (!ApplicationClientTypes.UsesClientSecret(app.ClientType))
            return;

        var providedSecret = (request.ClientSecret ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(providedSecret))
            throw new BusinessRuleValidationException("ClientSecretRequired", "client_secret is required for confidential clients.");

        if (!_hashingService.Verify(providedSecret, app.ApiSecretHash.Value))
            throw new UnauthorizedAccessException("Invalid client_secret.");
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

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
