using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Queries;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanCatalogDto>>
{
    private readonly ISubscriptionPlanRepository _planRepository;

    public GetSubscriptionPlansQueryHandler(ISubscriptionPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IReadOnlyList<SubscriptionPlanCatalogDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _planRepository.GetCatalogPlansAsync(request.ApplicationId, cancellationToken);
        var allRoleIds = plans.SelectMany(p => p.RoleAssignments.Select(a => a.RoleId));
        var names = await _planRepository.GetRoleNamesByIdsAsync(allRoleIds, cancellationToken);

        return plans.Select(p => SubscriptionEntityMapper.ToCatalogDto(p, names)).ToList();
    }
}
