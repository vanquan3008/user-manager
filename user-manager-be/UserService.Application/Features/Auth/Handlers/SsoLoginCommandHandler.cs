using MediatR;
using UserService.Application.Common;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public sealed class SsoLoginCommandHandler : IRequestHandler<SsoLoginCommand, SsoLoginResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly ITokenService _tokenService;

    public SsoLoginCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IApplicationMembershipRepository membershipRepository,
        ITokenService tokenService)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _tokenService = tokenService;
    }

    public async Task<SsoLoginResponse> Handle(SsoLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Login via identity service
        var (succeeded, identityUser, errors) = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!succeeded || identityUser == null)
            throw new UnauthorizedException(string.Join(", ", errors));

        // 2. Load domain user
        var domainUser = await _userRepository.GetByIdAsync(identityUser.UserProfileId!.Value, cancellationToken);
        if (domainUser == null)
            throw new EntityNotFoundException(nameof(Domain.Entities.User), identityUser.UserProfileId!.Value);

        // 3. Validate SSO account
        if (!domainUser.IsSSOAccount)
            throw new InvalidDomainObjectException(
                nameof(request.Email),
                request.Email,
                "This account uses the regular admin login. Please sign in from /login.");

        // 4. Check if user is admin (admins can access all apps)
        var roles = await _identityService.GetUserRolesAsync(identityUser.Id, cancellationToken);
        var isAdmin = roles.Contains(RoleNames.Admin) || roles.Contains(RoleNames.SuperAdmin);

        // 5. Get active application memberships
        var memberships = await _membershipRepository.GetByUserAsync(identityUser.Id, cancellationToken);
        var activeMemberships = memberships.Where(m => m.IsActive).ToList();
        
        if (!isAdmin && activeMemberships.Count == 0)
            throw new UnauthorizedException("SSO user has no active application access.");

        // 6. Generate auth-server session token only.
        var tokenRequest = new DTOs.Auth.AccessTokenRequest(
            identityUser.Id,
            identityUser.Email,
            domainUser.Name.DisplayName,
            roles,
            Permissions: new List<string>(),
            ApplicationId: null,
            Scope: null,
            SSOAppIds: null,
            OAuthScopes: null
        );

        var accessToken = _tokenService.GenerateAccessToken(tokenRequest);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 7. Save refresh token
        await _identityService.UpdateRefreshTokenAsync(
            identityUser.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7),
            cancellationToken);

        // 8. Build response
        var userDto = new SsoUserDto(
            identityUser.Id,
            identityUser.Email,
            identityUser.UserName,
            domainUser.Name.DisplayName,
            roles.ToList(),
            new List<string>() // No permissions in global SSO token
        );

        return new SsoLoginResponse(
            accessToken,
            refreshToken,
            userDto
        );
    }
}
