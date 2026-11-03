using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.Features.Users.Queries;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Users.Handlers;

/// <summary>
/// Handler for GetUsersQuery - retrieves paginated list of users with optional search.
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResponse<IdentityUserDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IIdentityService identityService, IUserRepository userRepository)
    {
        _identityService = identityService;
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<IdentityUserDto>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _identityService.GetUsersAsync(
            query.PageIndex,
            query.PageSize,
            query.Search,
            cancellationToken,
            query.SsoAccounts);

        var profileIds = items
            .Where(i => i.UserProfileId.HasValue)
            .Select(i => i.UserProfileId!.Value)
            .Distinct()
            .ToList();

        var ssoByProfile = await _userRepository.GetIsSSOAccountByProfileIdsAsync(profileIds, cancellationToken);

        var enriched = items.Select(i =>
        {
            var isSSO = i.UserProfileId is { } pid && ssoByProfile.TryGetValue(pid, out var flag) && flag;
            return i with { IsSSOAccount = i.IsSSOAccount || isSSO };
        }).ToList();

        return new PagedResponse<IdentityUserDto>(
            true,
            enriched,
            totalCount,
            query.PageIndex,
            query.PageSize);
    }
}
