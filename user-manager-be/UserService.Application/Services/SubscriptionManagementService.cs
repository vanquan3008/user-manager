using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Services;

public sealed class SubscriptionManagementService : ISubscriptionManagementService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionManagementService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository planRepository,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> ActivateAsync(
        ActivateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (activeSubscription != null)
        {
            throw new BusinessRuleValidationException("ActiveSubscriptionExists", "User already has an active subscription. Cancel it first.");
        }

        var plan = await GetActivePlanForApplicationAsync(
            command.SubscriptionPlanId,
            command.ApplicationId,
            cancellationToken);

        var features = await _planRepository.GetFeatureCodesByPlanIdAsync(plan.Id, cancellationToken);
        var subscription = Subscription.AssignPlan(
            Guid.NewGuid(),
            command.UserId,
            command.ApplicationId,
            plan,
            features,
            command.SubscriptionType,
            _clock.UtcNow,
            maxMembers: command.MaxMembers);

        if (command.ExpiresAt.HasValue)
            subscription.ExtendSubscription(command.ExpiresAt.Value, _clock.UtcNow);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }

    public async Task<SubscriptionDto> UpgradeAsync(
        UpgradeSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        var plan = await GetActivePlanForApplicationAsync(
            command.NewSubscriptionPlanId,
            subscription.ApplicationId,
            cancellationToken);

        var features = await _planRepository.GetFeatureCodesByPlanIdAsync(plan.Id, cancellationToken);
        var snapshot = plan.CreateEntitlementSnapshot(features, subscription.MaxMembers);
        subscription.UpgradeSubscription(command.NewType, command.NewSubscriptionPlanId, command.ExpiresAt, snapshot, _clock.UtcNow);

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }

    public async Task<SubscriptionDto> ExtendAsync(
        ExtendSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        subscription.ExtendSubscription(command.NewExpiresAt, _clock.UtcNow);

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }

    public async Task CancelAsync(
        CancelSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        subscription.CancelSubscription(_clock.UtcNow);

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubscriptionPlanCatalogDto> CreatePlanAsync(
        CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var featureIds = await _planRepository.ResolveFeatureIdsAsync(command.Features, cancellationToken);
        var permissionIds = await _planRepository.ResolvePermissionIdsAsync(
            command.Permissions ?? Array.Empty<string>(),
            cancellationToken);

        var plan = SubscriptionPlan.Create(
            Guid.NewGuid(),
            command.ApplicationId,
            PlanName.Create(command.Name),
            PlanDescription.Create(command.Description),
            Money.Create(command.MonthlyPrice),
            Money.Create(command.YearlyPrice),
            command.MaxUsers);

        plan.UpdateLimits(command.MaxUsers, command.MaxApiCalls);
        plan.SetIcon(command.Icon != null ? UrlValue.Create(command.Icon) : null);
        plan.SetDisplayOrder(command.DisplayOrder);
        plan.ReplaceFeatureAssignments(featureIds);
        plan.ReplacePermissionAssignments(permissionIds);

        if (!command.IsActive)
            plan.Deactivate();

        await _planRepository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildPlanCatalogDtoAsync(plan, cancellationToken);
    }

    public async Task<SubscriptionPlanCatalogDto> UpdatePlanAsync(
        UpdateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdWithCatalogAsync(command.PlanId, cancellationToken);
        if (plan == null)
            throw new KeyNotFoundException($"Subscription plan '{command.PlanId}' not found.");
        if (plan.ApplicationId != command.ApplicationId)
            throw new InvalidOperationException("ApplicationId does not match this subscription plan.");

        var featureIds = await _planRepository.ResolveFeatureIdsAsync(command.Features, cancellationToken);
        var permissionIds = await _planRepository.ResolvePermissionIdsAsync(
            command.Permissions ?? Array.Empty<string>(),
            cancellationToken);

        plan.UpdatePlan(
            PlanName.Create(command.Name.Trim()),
            PlanDescription.Create(command.Description ?? string.Empty),
            Money.Create(command.MonthlyPrice),
            Money.Create(command.YearlyPrice));
        plan.UpdateLimits(command.MaxUsers, command.MaxApiCalls);
        plan.SetDisplayOrder(command.DisplayOrder);
        plan.SetIcon(command.Icon != null ? UrlValue.Create(command.Icon) : null);
        plan.ReplaceFeatureAssignments(featureIds);
        plan.ReplacePermissionAssignments(permissionIds);

        if (command.IsActive)
            plan.Activate();
        else
            plan.Deactivate();

        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildPlanCatalogDtoAsync(plan, cancellationToken);
    }

    public async Task DeactivatePlanAsync(
        DeactivateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(command.PlanId, cancellationToken);
        if (plan == null)
            throw new KeyNotFoundException($"Subscription plan '{command.PlanId}' not found.");

        plan.Deactivate();
        await _planRepository.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Subscription> GetActiveSubscriptionForUpdateAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetActiveSubscriptionForUpdateAsync(
            userId,
            applicationId,
            cancellationToken);

        if (subscription == null)
            throw new BusinessRuleValidationException("NoActiveSubscription", "No active subscription found");

        return subscription;
    }

    private async Task<SubscriptionPlan> GetActivePlanForApplicationAsync(
        Guid planId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(planId, cancellationToken);
        if (plan == null)
            throw new BusinessRuleValidationException("SubscriptionPlanNotFound", $"Subscription plan '{planId}' not found.");
        if (!plan.IsActive)
            throw new BusinessRuleValidationException("SubscriptionPlanInactive", "Subscription plan is inactive.");
        if (!plan.ApplicationId.HasValue || plan.ApplicationId.Value != applicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "Subscription plan must belong to the same application.");

        return plan;
    }

    private async Task<SubscriptionPlanCatalogDto> BuildPlanCatalogDtoAsync(
        SubscriptionPlan plan,
        CancellationToken cancellationToken)
    {
        var features = await _planRepository.GetFeatureCodesByPlanIdAsync(plan.Id, cancellationToken);
        var permissions = await _planRepository.GetPermissionCodesByPlanIdAsync(plan.Id, cancellationToken);
        return SubscriptionEntityMapper.ToCatalogDto(plan, features, permissions);
    }
}
