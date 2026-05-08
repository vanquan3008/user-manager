using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanCatalogDto>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionPlanCatalogDto> Handle(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var roleIds = command.AssignedRoleIds ?? Array.Empty<Guid>();

        var plan = SubscriptionPlan.Create(
            Guid.NewGuid(),
            command.ApplicationId,
            PlanName.Create(command.Name),
            PlanDescription.Create(command.Description),
            Money.Create(command.MonthlyPrice),
            Money.Create(command.YearlyPrice),
            command.MaxUsers,
            command.IncludeAnalytics,
            command.IncludeSupport);

        plan.UpdateFeatures(
            command.IncludeAnalytics,
            command.IncludeSupport,
            command.IncludeAdvancedSecurity,
            command.IncludeCustomDomain);

        plan.UpdateLimits(command.MaxUsers, command.MaxApiCalls);
        plan.ReplaceRoleAssignments(roleIds);
        plan.SetIcon(command.Icon != null ? UrlValue.Create(command.Icon) : null);
        plan.SetDisplayOrder(command.DisplayOrder);

        if (!command.IsActive)
            plan.Deactivate();

        await _planRepository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var names = await _planRepository.GetRoleNamesByIdsAsync(
            plan.RoleAssignments.Select(a => a.RoleId),
            cancellationToken);

        return SubscriptionEntityMapper.ToCatalogDto(plan, names);
    }
}
