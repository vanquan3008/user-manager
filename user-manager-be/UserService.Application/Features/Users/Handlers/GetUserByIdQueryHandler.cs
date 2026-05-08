using MediatR;
using UserService.Application.Features.Users.Queries;
using UserService.Application.Mapping;
using UserService.Application.Services;

namespace UserService.Application.Features.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserWithRolesDto?>
{
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserWithRolesDto?> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return null;

        var roles = await _identityService.GetUserRolesAsync(query.UserId, cancellationToken);

        return UserDtoMapper.ToUserWithRoles(user, roles);
    }
}
