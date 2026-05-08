using System.Security.Cryptography;
using System.Text;
using MediatR;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.DTOs.OAuth;
using UserService.Application.Features.OAuth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Domain.Constants;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.OAuth.Handlers;

public sealed class ExchangeOAuthCodeCommandHandler : IRequestHandler<ExchangeOAuthCodeCommand, OAuthTokenResponse>
{
    private readonly IApplicationRepository _applications;
    private readonly IOAuthAuthorizationCodeRepository _codes;
    private readonly IOAuthRefreshTokenRepository _refreshTokens;
    private readonly IIdentityService _identity;
    private readonly IUserPermissionService _userPermissions;
    private readonly ITokenService _tokenService;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IUserApplicationRepository _userApplications;
    private readonly AppSettings _appSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ExchangeOAuthCodeCommandHandler(
        IApplicationRepository applications,
        IOAuthAuthorizationCodeRepository codes,
        IOAuthRefreshTokenRepository refreshTokens,
        IIdentityService identity,
        IUserPermissionService userPermissions,
        ITokenService tokenService,
        ISubscriptionRepository subscriptions,
        IUserApplicationRepository userApplications,
        AppSettings appSettings,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _applications = applications;
        _codes = codes;
        _refreshTokens = refreshTokens;
        _identity = identity;
        _userPermissions = userPermissions;
        _tokenService = tokenService;
        _subscriptions = subscriptions;
        _userApplications = userApplications;
        _appSettings = appSettings;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<OAuthTokenResponse> Handle(ExchangeOAuthCodeCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var grantType = (req.GrantType ?? string.Empty).Trim();
        var app = await _applications.GetByApiKeyAsync(req.ClientId.Trim(), cancellationToken);
        if (app == null || !app.IsActive)
            throw new BusinessRuleValidationException("OAuthClientInactive", "OAuth client not found or inactive.");

        if (string.Equals(grantType, "authorization_code", StringComparison.OrdinalIgnoreCase))
            return await ExchangeAuthorizationCodeAsync(app.Id, req, cancellationToken);

        if (string.Equals(grantType, "refresh_token", StringComparison.OrdinalIgnoreCase))
            return await ExchangeRefreshTokenAsync(app.Id, req, cancellationToken);
        throw new BusinessRuleValidationException("GrantTypeUnsupported", "Unsupported grant_type.");
    }

    private async Task<OAuthTokenResponse> ExchangeAuthorizationCodeAsync(Guid appId, OAuthTokenRequest req, CancellationToken cancellationToken)
    {
        var codeValue = req.Code!.Trim();
        var entity = await _codes.GetByCodeAsync(codeValue, cancellationToken);
        if (entity == null)
            throw new UnauthorizedAccessException("Invalid authorization code.");

        if (entity.ApplicationId != appId)
            throw new UnauthorizedAccessException("Authorization code does not belong to this client.");

        var redirectUri = req.RedirectUri?.Trim() ?? string.Empty;
        if (!string.Equals(entity.RedirectUri, redirectUri, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("redirect_uri mismatch.");

        if (entity.IsConsumed)
            throw new UnauthorizedAccessException("Authorization code already consumed.");

        if (entity.IsExpired(_clock.UtcNow))
            throw new UnauthorizedAccessException("Authorization code expired.");

        if (!string.Equals(entity.CodeChallengeMethod, "S256", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleValidationException("PkceMethodUnsupported", "Unsupported PKCE method.");

        var verifier = req.CodeVerifier!.Trim();

        var expectedChallenge = ComputeS256Challenge(verifier);
        if (!string.Equals(expectedChallenge, entity.CodeChallenge, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid code_verifier.");

        entity.Consume(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _identity.GetUserByIdAsync(entity.UserId, cancellationToken);
        if (user == null)
            throw new BusinessRuleValidationException("UserNotFound", "User not found.");

        // 1. Fetch Global Roles
        var roles = await _identity.GetUserRolesAsync(entity.UserId, cancellationToken);
        var isSystemAdmin = roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                          string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(r, "SA", StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
        {
            var membership = await _userApplications.GetByUserAndAppAsync(entity.UserId, appId, cancellationToken);
            if (membership == null || !membership.IsActive)
                throw new BusinessRuleValidationException("NotAppMember", "User is not an active member of this application.");
        }

        // 2. Fetch App-scoped Roles & Permissions
        var permissions = await _userPermissions.GetPermissionCodesForUserAsync(entity.UserId, appId, cancellationToken);

        // 3. Optional: Check for active subscription/license
        var activeSub = await _subscriptions.GetActiveSubscriptionAsync(entity.UserId, appId, cancellationToken);
        // Notes: You could block login if subscription is required but missing, 
        // or just mark the token with a claim.

        var oauthScopes = OAuthScopeNames.FilterDefaultScopes(
            entity.Scopes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var accessToken = _tokenService.GenerateAccessToken(
            entity.UserId,
            user.Email,
            user.UserName,
            roles,
            permissions,
            appId,
            scope: null,
            ssoAppIds: null,
            oauthScopes: oauthScopes);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = ComputeSha256Hex(refreshToken);

        await _refreshTokens.RevokeAllForUserClientAsync(entity.UserId, appId, cancellationToken);
        await _refreshTokens.AddAsync(
            UserService.Domain.Entities.OAuthRefreshToken.Create(
                Guid.NewGuid(),
                entity.UserId,
                appId,
                UserService.Domain.ValueObjects.TokenValue.Create(refreshTokenHash),
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

    private static string ComputeS256Challenge(string verifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return OAuthHelpers.Base64UrlEncode(bytes);
    }

    private async Task<OAuthTokenResponse> ExchangeRefreshTokenAsync(Guid appId, OAuthTokenRequest req, CancellationToken cancellationToken)
    {
        var refreshToken = (req.RefreshToken ?? string.Empty).Trim();
        // required by validator
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new BusinessRuleValidationException("RefreshTokenRequired", "Missing refresh_token.");

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

        // rotate
        entity.Revoke(_clock.UtcNow);

        var user = await _identity.GetUserByIdAsync(entity.UserId, cancellationToken);
        if (user == null)
            throw new BusinessRuleValidationException("UserNotFound", "User not found.");

        var roles = await _identity.GetUserRolesAsync(entity.UserId, cancellationToken);
        var isSystemAdmin = roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                          string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(r, "SA", StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
        {
            var membership = await _userApplications.GetByUserAndAppAsync(entity.UserId, appId, cancellationToken);
            if (membership == null || !membership.IsActive)
                throw new BusinessRuleValidationException("NotAppMember", "User is not an active member of this application.");
        }

        var permissions = await _userPermissions.GetPermissionCodesForUserAsync(entity.UserId, appId, cancellationToken);
        var oauthScopes = OAuthScopeNames.FilterDefaultScopes(
            entity.Scopes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var accessToken = _tokenService.GenerateAccessToken(
            entity.UserId,
            user.Email,
            user.UserName,
            roles,
            permissions,
            appId,
            scope: null,
            ssoAppIds: null,
            oauthScopes: oauthScopes);

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newHash = ComputeSha256Hex(newRefreshToken);

        await _refreshTokens.AddAsync(
            UserService.Domain.Entities.OAuthRefreshToken.Create(
                Guid.NewGuid(),
                entity.UserId,
                appId,
                UserService.Domain.ValueObjects.TokenValue.Create(newHash),
                string.Join(" ", oauthScopes),
                _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OAuthTokenResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: (int)TimeSpan.FromMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes).TotalSeconds,
            RefreshToken: newRefreshToken,
            Scopes: oauthScopes.ToArray(),
            User: new OAuthUserDto(
                user.Id,
                user.Email,
                user.UserName,
                user.UserName,
                roles));
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}

