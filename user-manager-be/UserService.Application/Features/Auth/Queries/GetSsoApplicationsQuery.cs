using MediatR;

namespace UserService.Application.Features.Auth.Queries;

/// <summary>
/// Query to get list of applications available for SSO user.
/// Used for SSO discovery - showing user which apps they can access.
/// </summary>
public sealed record GetSsoApplicationsQuery(
    Guid UserId
) : IRequest<GetSsoApplicationsResponse>;

public sealed record GetSsoApplicationsResponse(
    IReadOnlyList<SsoApplicationInfo> Applications
);

public sealed record SsoApplicationInfo(
    Guid ApplicationId,
    string ApplicationName,
    string? LogoUrl,
    string? Description,
    string Role,
    bool IsActive
);
