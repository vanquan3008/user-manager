using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Mapping;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

/// <summary>
/// Single place where authenticated sessions are turned into token responses.
/// Every response is backed by a server-side <see cref="UserSession"/>; the access token carries its id (<c>sid</c>).
/// </summary>
public sealed class AuthResponseFactory
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly ISessionService _sessions;
    private readonly IMfaChallengeService _mfaChallenges;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;
    private readonly AppSettings _appSettings;

    public AuthResponseFactory(
        IIdentityService identityService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IApplicationAccessService applicationAccessService,
        ISessionService sessions,
        IMfaChallengeService mfaChallenges,
        IAuditLogger audit,
        IClock clock,
        AppSettings appSettings)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _applicationAccessService = applicationAccessService;
        _sessions = sessions;
        _mfaChallenges = mfaChallenges;
        _audit = audit;
        _clock = clock;
        _appSettings = appSettings;
    }

    /// <summary>
    /// Completes a first-factor sign-in: returns an MFA challenge when the account has MFA enabled,
    /// otherwise opens a session and issues tokens.
    /// </summary>
    public async Task<AuthResponse> CompleteConsoleSignInAsync(
        IdentityUserDto user,
        Guid? applicationId,
        string authenticationMethod,
        CancellationToken cancellationToken = default)
    {
        if (user.TwoFactorEnabled)
        {
            var (token, expiresAt) = _mfaChallenges.Create(new MfaChallenge(user.Id, UserSessionKinds.Console, authenticationMethod, applicationId));
            await _audit.LogAsync(AuditEventTypes.LoginMfaChallenged, true, new AuditEntry(ActorUserId: user.Id, ActorEmail: user.Email), cancellationToken);
            var domainUser = await RequireDomainUserAsync(user.Id, cancellationToken);
            return new AuthResponse(string.Empty, null, expiresAt, ToUserDto(domainUser, user, Array.Empty<string>()),
                MfaRequired: true, MfaToken: token, MfaExpiresAt: expiresAt);
        }

        return await StartConsoleSessionAsync(user.Id, applicationId, authenticationMethod, cancellationToken);
    }

    public async Task<AuthResponse> StartConsoleSessionAsync(
        Guid userId,
        Guid? applicationId,
        string authenticationMethod,
        CancellationToken cancellationToken = default)
    {
        var (roles, permissions) = await ResolveAccessAsync(userId, applicationId, cancellationToken);
        var issued = await _sessions.StartAsync(userId, UserSessionKinds.Console, authenticationMethod, applicationId, cancellationToken);
        await _identityService.RecordSuccessfulLoginAsync(userId, cancellationToken);
        await _audit.LogAsync(
            authenticationMethod == AuthenticationMethods.MagicLink ? AuditEventTypes.MagicLinkLogin : AuditEventTypes.LoginSucceeded,
            true,
            new AuditEntry(ActorUserId: userId, SessionId: issued.Session.Id, ApplicationId: applicationId, Details: new { method = authenticationMethod }),
            cancellationToken);
        return await BuildConsoleResponseAsync(userId, issued, roles, permissions, cancellationToken);
    }

    /// <summary>Re-issues tokens for a session after refresh-token rotation.</summary>
    public async Task<AuthResponse> BuildRefreshedResponseAsync(IssuedSession issued, CancellationToken cancellationToken = default)
    {
        if (issued.Session.SessionKind == UserSessionKinds.HostedSso)
        {
            if (!await _identityService.IsSignInAllowedAsync(issued.Session.UserId, cancellationToken))
                throw new AuthenticationFlowException(AuthErrorCodes.AccountDisabled, "This account is disabled or locked.");

            var sso = await BuildHostedSsoResponseAsync(issued, cancellationToken);
            var appUser = await _identityService.GetUserByIdAsync(issued.Session.UserId, cancellationToken)
                          ?? throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "User not found.");
            var domainUser = await RequireDomainUserAsync(issued.Session.UserId, cancellationToken);
            return new AuthResponse(
                sso.AccessToken,
                sso.RefreshToken,
                _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes),
                ToUserDto(domainUser, appUser, sso.User.Roles),
                SessionId: issued.Session.Id);
        }

        var (roles, permissions) = await ResolveAccessAsync(issued.Session.UserId, issued.Session.ApplicationId, cancellationToken);
        return await BuildConsoleResponseAsync(issued.Session.UserId, issued, roles, permissions, cancellationToken);
    }

    public async Task<SsoLoginResponse> CompleteHostedSsoSignInAsync(
        IdentityUserDto user,
        string authenticationMethod,
        CancellationToken cancellationToken = default)
    {
        if (user.TwoFactorEnabled)
        {
            var (token, expiresAt) = _mfaChallenges.Create(new MfaChallenge(user.Id, UserSessionKinds.HostedSso, authenticationMethod, null));
            await _audit.LogAsync(AuditEventTypes.LoginMfaChallenged, true, new AuditEntry(ActorUserId: user.Id, ActorEmail: user.Email), cancellationToken);
            var domainUser = await RequireDomainUserAsync(user.Id, cancellationToken);
            return new SsoLoginResponse(string.Empty, null,
                new SsoUserDto(user.Id, user.Email, user.UserName, domainUser.Name.DisplayName, Array.Empty<string>(), Array.Empty<string>()),
                MfaRequired: true, MfaToken: token, MfaExpiresAt: expiresAt);
        }

        return await StartHostedSsoSessionAsync(user.Id, authenticationMethod, cancellationToken);
    }

    public async Task<SsoLoginResponse> StartHostedSsoSessionAsync(Guid userId, string authenticationMethod, CancellationToken cancellationToken = default)
    {
        var issued = await _sessions.StartAsync(userId, UserSessionKinds.HostedSso, authenticationMethod, null, cancellationToken);
        await _identityService.RecordSuccessfulLoginAsync(userId, cancellationToken);
        await _audit.LogAsync(AuditEventTypes.LoginSucceeded, true,
            new AuditEntry(ActorUserId: userId, SessionId: issued.Session.Id, Details: new { method = authenticationMethod, kind = UserSessionKinds.HostedSso }),
            cancellationToken);
        return await BuildHostedSsoResponseAsync(issued, cancellationToken);
    }

    public async Task<SsoLoginResponse> BuildHostedSsoResponseAsync(IssuedSession issued, CancellationToken cancellationToken = default)
    {
        var userId = issued.Session.UserId;
        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken)
                      ?? throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "User not found.");
        var domainUser = await RequireDomainUserAsync(userId, cancellationToken);
        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);

        // The hosted SSO token only authenticates the user to the auth center (authorize/consent);
        // it carries no application permissions.
        var accessToken = _tokenService.GenerateAccessToken(new AccessTokenRequest(
            userId,
            appUser.Email,
            domainUser.Name.DisplayName,
            roles,
            Permissions: Array.Empty<string>(),
            SessionId: issued.Session.Id,
            AuthenticationMethod: issued.Session.AuthenticationMethod));

        return new SsoLoginResponse(
            accessToken,
            issued.RefreshToken,
            new SsoUserDto(userId, appUser.Email, appUser.UserName, domainUser.Name.DisplayName, roles.ToList(), Array.Empty<string>()),
            SessionId: issued.Session.Id);
    }

    private async Task<(IList<string> Roles, IReadOnlyList<string> Permissions)> ResolveAccessAsync(
        Guid userId, Guid? applicationId, CancellationToken cancellationToken)
    {
        if (!await _identityService.IsSignInAllowedAsync(userId, cancellationToken))
            throw new AuthenticationFlowException(AuthErrorCodes.AccountDisabled, "This account is disabled or locked.");

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        if (applicationId.HasValue &&
            !await _applicationAccessService.HasAccessAsync(userId, applicationId.Value, roles, cancellationToken))
        {
            throw new UnauthorizedException("User does not have active access to this application.");
        }

        var permissions = applicationId.HasValue
            ? await _userPermissionService.GetPermissionCodesForUserAsync(userId, applicationId.Value, cancellationToken)
            : await _userPermissionService.GetPermissionCodesForUserAsync(userId, cancellationToken);
        return (roles, permissions);
    }

    private async Task<AuthResponse> BuildConsoleResponseAsync(
        Guid userId,
        IssuedSession issued,
        IList<string> roles,
        IReadOnlyList<string> permissions,
        CancellationToken cancellationToken)
    {
        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken)
                      ?? throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "User not found.");
        var domainUser = await RequireDomainUserAsync(userId, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(new AccessTokenRequest(
            userId,
            appUser.Email,
            appUser.UserName,
            roles,
            permissions,
            issued.Session.ApplicationId,
            SessionId: issued.Session.Id,
            AuthenticationMethod: issued.Session.AuthenticationMethod));

        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);
        return new AuthResponse(accessToken, issued.RefreshToken, expiresAt, ToUserDto(domainUser, appUser, roles), SessionId: issued.Session.Id);
    }

    private async Task<User> RequireDomainUserAsync(Guid userId, CancellationToken cancellationToken)
        => await _userRepository.GetByIdAsync(userId, cancellationToken)
           ?? throw new EntityNotFoundException(nameof(User), userId);

    private static DTOs.Users.UserDto ToUserDto(User domainUser, IdentityUserDto appUser, IEnumerable<string> roles)
        => UserDtoMapper.ToDto(domainUser) with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };
}
