using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

/// <summary>
/// Handles SSO authorization - exchanges global SSO token for application-specific token.
/// This is called after SSO login when user selects which application to access.
/// </summary>
public sealed class SsoAuthorizeCommandHandler : IRequestHandler<SsoAuthorizeCommand, SsoAuthorizeResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly IUserPermissionService _userPermissionService;
    private readonly ITokenService _tokenService;

    public SsoAuthorizeCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IApplicationMembershipRepository membershipRepository,
        IUserPermissionService userPermissionService,
        ITokenService tokenService)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _membershipRepository = membershipRepository;
        _userPermissionService = userPermissionService;
        _tokenService = tokenService;
    }

    public async Task<SsoAuthorizeResponse> Handle(SsoAuthorizeCommand request, CancellationToken cancellationToken)
    {
        // 1. Load identity user
        var identityUser = await _identityService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (identityUser == null)
            throw new UnauthorizedException("User not found.");

        // 2. Load domain user
        var domainUser = await _userRepository.GetByIdAsync(identityUser.UserProfileId!.Value, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.User), identityUser.UserProfileId!.Value);

        // 3. Validate SSO account
        if (!domainUser.IsSSOAccount)
            throw new InvalidOperationException("This is not an SSO account.");

        // 4. Validate application exists and is active
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        if (application == null || !application.IsActive)
            throw new EntityNotFoundException(nameof(AppApplication), request.ApplicationId);

        // 5. Check if user is admin (admins can access all apps)
        var roles = await _identityService.GetUserRolesAsync(identityUser.Id, cancellationToken);
        var isAdmin = roles.Contains(RoleNames.Admin) || roles.Contains(RoleNames.SuperAdmin);

        // 6. Validate user has access to this application
        if (!isAdmin)
        {
            var membership = await _membershipRepository.GetByUserAndAppAsync(
                identityUser.Id,
                request.ApplicationId,
                cancellationToken);

            if (membership == null || !membership.IsActive)
                throw new UnauthorizedException($"User does not have access to application '{application.Name}'.");
        }

        // 7. Get user permissions for this application
        var permissions = await _userPermissionService.GetPermissionCodesForUserAsync(
            identityUser.Id,
            request.ApplicationId,
            cancellationToken);

        // 8. Generate application-specific token
        var tokenRequest = new AccessTokenRequest(
            identityUser.Id,
            identityUser.Email,
            domainUser.Name.DisplayName,
            roles,
            permissions,
            request.ApplicationId,
            Scope: null,
            SSOAppIds: null,
            OAuthScopes: null
        );

        var accessToken = _tokenService.GenerateAccessToken(tokenRequest);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 9. Save refresh token
        await _identityService.UpdateRefreshTokenAsync(
            identityUser.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7),
            cancellationToken);

        // 10. Build redirect URL if provided
        var redirectUrl = !string.IsNullOrWhiteSpace(request.ReturnUrl)
            ? request.ReturnUrl
            : null;

        return new SsoAuthorizeResponse(
            accessToken,
            refreshToken,
            redirectUrl
        );
    }
}

