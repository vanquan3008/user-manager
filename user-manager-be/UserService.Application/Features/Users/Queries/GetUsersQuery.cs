using MediatR;

namespace UserService.Application.Features.Users.Queries;

public record GetUsersQuery(
    int PageIndex,
    int PageSize,
    string? Search = null
) : IRequest<PagedResponse<IdentityUserDto>>;
