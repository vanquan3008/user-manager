using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Auth.Queries;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Auth.Handlers;

public sealed class GetSsoApplicationsQueryHandler : IRequestHandler<GetSsoApplicationsQuery, GetSsoApplicationsResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationMembershipRepository _membershipRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetSsoApplicationsQueryHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IApplicationMembershipRepository membershipRepository,
        IApplicationRepository applicationRepository)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<GetSsoApplicationsResponse> Handle(GetSsoApplicationsQuery request, CancellationToken cancellationToken)
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

        // 4. Check if user is admin (admins can access all apps)
        var roles = await _identityService.GetUserRolesAsync(identityUser.Id, cancellationToken);
        var isAdmin = roles.Contains(RoleNames.Admin) || roles.Contains(RoleNames.SuperAdmin);

        // 5. Get active application memberships
        var memberships = await _membershipRepository.GetByUserAsync(identityUser.Id, cancellationToken);
        var activeMemberships = memberships.Where(m => m.IsActive).ToList();

        if (!isAdmin && activeMemberships.Count == 0)
        {
            return new GetSsoApplicationsResponse(Array.Empty<SsoApplicationInfo>());
        }

        // 6. Load applications
        var applicationIds = activeMemberships.Select(m => m.ApplicationId).Distinct().ToList();
        var applications = new List<AppApplication>();
        foreach (var appId in applicationIds)
        {
            var app = await _applicationRepository.GetByIdAsync(appId, cancellationToken);
            if (app != null)
                applications.Add(app);
        }

        // 7. Build response
        var ssoApplications = new List<SsoApplicationInfo>();
        foreach (var membership in activeMemberships)
        {
            var app = applications.FirstOrDefault(a => a.Id == membership.ApplicationId);
            if (app != null && app.IsActive)
            {
                ssoApplications.Add(new SsoApplicationInfo(
                    app.Id,
                    app.Name,
                    app.LogoUrl,
                    app.Description,
                    "Member", // TODO: Load role name from RoleId
                    membership.IsActive
                ));
            }
        }

        return new GetSsoApplicationsResponse(ssoApplications);
    }
}
