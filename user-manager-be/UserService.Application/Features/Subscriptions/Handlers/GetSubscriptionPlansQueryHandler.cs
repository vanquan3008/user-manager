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
        var featuresByPlan = await _planRepository.GetFeatureCodesByPlanIdsAsync(
            plans.Select(plan => plan.Id),
            cancellationToken);
        var permissionsByPlan = await _planRepository.GetPermissionCodesByPlanIdsAsync(
            plans.Select(plan => plan.Id),
            cancellationToken);

        return plans
            .Select(plan => SubscriptionEntityMapper.ToCatalogDto(
                plan,
                featuresByPlan.TryGetValue(plan.Id, out var features) ? features : Array.Empty<string>(),
                permissionsByPlan.TryGetValue(plan.Id, out var permissions) ? permissions : Array.Empty<string>()))
            .ToList();
    }
}
