using MediatR;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, SubscriptionPlanCatalogDto>
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSubscriptionPlanCommandHandler(
        ISubscriptionPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionPlanCatalogDto> Handle(UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdWithRoleAssignmentsAsync(command.PlanId, cancellationToken);
        if (plan == null)
            throw new KeyNotFoundException($"Subscription plan '{command.PlanId}' not found.");
        if (plan.ApplicationId != command.ApplicationId)
            throw new InvalidOperationException("ApplicationId does not match this subscription plan.");

        var roleIds = command.AssignedRoleIds ?? Array.Empty<Guid>();

        plan.UpdatePlan(
            PlanName.Create(command.Name.Trim()),
            PlanDescription.Create(command.Description ?? string.Empty),
            Money.Create(command.MonthlyPrice),
            Money.Create(command.YearlyPrice));
        plan.UpdateFeatures(command.IncludeAnalytics, command.IncludeSupport, command.IncludeAdvancedSecurity, command.IncludeCustomDomain);
        plan.UpdateLimits(command.MaxUsers, command.MaxApiCalls);
        plan.ReplaceRoleAssignments(roleIds);
        plan.SetDisplayOrder(command.DisplayOrder);
        plan.SetIcon(command.Icon != null ? UrlValue.Create(command.Icon) : null);
        if (command.IsActive) plan.Activate();
        else plan.Deactivate();

        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var names = await _planRepository.GetRoleNamesByIdsAsync(
            plan.RoleAssignments.Select(a => a.RoleId),
            cancellationToken);

        return SubscriptionEntityMapper.ToCatalogDto(plan, names);
    }
}
