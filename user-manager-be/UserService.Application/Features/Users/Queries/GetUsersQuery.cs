using MediatR;

namespace UserService.Application.Features.Users.Queries;

public record GetUsersQuery(
    int PageIndex,
    int PageSize,
    string? Search = null,
    /// <summary>true = SSO accounts only, false = console accounts only, null = all.</summary>
    bool? SsoAccounts = null
) : IRequest<PagedResponse<IdentityUserDto>>;
