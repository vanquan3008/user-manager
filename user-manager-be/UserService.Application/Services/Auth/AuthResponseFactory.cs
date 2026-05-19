using UserService.Application.Configuration;
using UserService.Application.DTOs.Auth;
using UserService.Application.Mapping;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Services.Auth;

public sealed class AuthResponseFactory
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserPermissionService _userPermissionService;
    private readonly IApplicationAccessService _applicationAccessService;
    private readonly IApplicationAppService _applicationService;
    private readonly IApplicationMembershipService _membershipService;
    private readonly IClock _clock;
    private readonly AppSettings _appSettings;

    public AuthResponseFactory(
        IIdentityService identityService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserPermissionService userPermissionService,
        IApplicationAccessService applicationAccessService,
        IApplicationAppService applicationService,
        IApplicationMembershipService membershipService,
        IClock clock,
        AppSettings appSettings)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _userPermissionService = userPermissionService;
        _applicationAccessService = applicationAccessService;
        _applicationService = applicationService;
        _membershipService = membershipService;
        _clock = clock;
        _appSettings = appSettings;
    }

    public async Task<AuthResponse> BuildUserAuthResponseAsync(
        Guid userId,
        Guid? applicationId,
        CancellationToken cancellationToken = default)
    {
        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken);
        if (appUser == null)
            throw new UnauthorizedException("User not found");

        var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(User), userId);

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        if (applicationId.HasValue)
        {
            var hasAccess = await _applicationAccessService.HasAccessAsync(
                userId,
                applicationId.Value,
                roles,
                cancellationToken);

            if (!hasAccess)
                throw new UnauthorizedException("User does not have active access to this application.");
        }

        var permissions = applicationId.HasValue
            ? await _userPermissionService.GetPermissionCodesForUserAsync(userId, applicationId.Value, cancellationToken)
            : await _userPermissionService.GetPermissionCodesForUserAsync(userId, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(new AccessTokenRequest(
            userId,
            appUser.Email,
            appUser.UserName,
            roles,
            permissions,
            applicationId));

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);

        await _identityService.UpdateRefreshTokenAsync(
            userId,
            refreshToken,
            _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        var userDto = UserDtoMapper.ToDto(domainUser) with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(accessToken, refreshToken, expiresAt, userDto);
    }

    public async Task<AuthResponse> BuildSSOAuthResponseAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var appUser = await _identityService.GetUserByIdAsync(userId, cancellationToken);
        if (appUser == null)
            throw new UnauthorizedException("User not found");

        var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(User), userId);

        var roles = await _identityService.GetUserRolesAsync(userId, cancellationToken);
        var permissions = await _userPermissionService.GetPermissionCodesForUserAsync(userId, cancellationToken);

        var isSystemAdmin = RoleNames.ContainsSuperAdmin(roles);
        var userApps = await _membershipService.GetByUserAsync(userId, cancellationToken);
        var activeMemberships = new List<ApplicationMembership>();

        foreach (var membership in userApps.Where(item => item.IsActive))
        {
            var hasAccess = await _applicationAccessService.HasAccessAsync(
                userId,
                membership.ApplicationId,
                roles,
                cancellationToken);

            if (hasAccess)
                activeMemberships.Add(membership);
        }

        var ssoAppIds = activeMemberships
            .Select(item => item.ApplicationId)
            .Distinct()
            .ToList();

        if (!isSystemAdmin && ssoAppIds.Count == 0)
            throw new UnauthorizedException("SSO account has no authorized applications. Contact an administrator.");

        var applications = new List<ApplicationInfoDto>();
        foreach (var membership in activeMemberships)
        {
            var app = await _applicationService.GetByIdAsync(membership.ApplicationId, cancellationToken);
            if (app != null && app.IsActive)
            {
                applications.Add(new ApplicationInfoDto(
                    app.Id,
                    app.Name,
                    app.Description));
            }
        }

        var accessToken = _tokenService.GenerateAccessToken(new AccessTokenRequest(
            userId,
            appUser.Email,
            appUser.UserName,
            roles,
            permissions,
            ApplicationId: null,
            Scope: null,
            SSOAppIds: ssoAppIds));

        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _clock.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpirationMinutes);

        await _identityService.UpdateRefreshTokenAsync(
            userId,
            refreshToken,
            _clock.UtcNow.AddDays(_appSettings.Jwt.RefreshTokenExpirationDays),
            cancellationToken);

        var userDto = UserDtoMapper.ToDto(domainUser) with
        {
            IsEmailVerified = appUser.EmailConfirmed,
            IsActive = appUser.IsActive,
            Roles = roles
        };

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            userDto,
            RequiresEmailVerification: false,
            Applications: applications);
    }
}
