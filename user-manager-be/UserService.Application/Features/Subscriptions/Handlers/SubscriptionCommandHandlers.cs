using MediatR;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.Repositories;
using UserService.Domain.Exceptions;
using UserService.Domain;
using UserService.Application.DTOs.Subscriptions;
using UserService.Application.Mapping;

namespace UserService.Application.Features.Subscriptions.Handlers;

public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        ISubscriptionPlanRepository planRepository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _planRepository = planRepository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(ActivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var activeSubscription = await _repository.GetActiveSubscriptionAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (activeSubscription != null)
        {
            throw new BusinessRuleValidationException("ActiveSubscriptionExists", "User already has an active subscription. Cancel it first.");
        }

        var plan = await _planRepository.GetByIdWithRoleAssignmentsAsync(command.SubscriptionPlanId, cancellationToken);
        if (plan == null)
            throw new BusinessRuleValidationException("SubscriptionPlanNotFound", $"Subscription plan '{command.SubscriptionPlanId}' not found.");
        if (!plan.ApplicationId.HasValue || plan.ApplicationId.Value != command.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "License package must belong to the same application.");

        var subscription = Subscription.AssignPlan(
            Guid.NewGuid(),
            command.UserId,
            command.ApplicationId,
            plan,
            command.SubscriptionType,
            _clock.UtcNow,
            maxMembers: command.MaxMembers);

        if (command.ExpiresAt.HasValue)
        {
            subscription.ExtendSubscription(command.ExpiresAt.Value, _clock.UtcNow);
        }

        await _repository.AddAsync(subscription, cancellationToken);

        var roleNames = await _planRepository.GetRoleNamesByIdsAsync(
            plan.RoleAssignments.Select(a => a.RoleId),
            cancellationToken);
        var targetRoles = AssignedRolesFormat.Normalize(plan.ToAssignedRolesCsv(roleNames));
        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (userApp is null)
        {
            userApp = UserApplication.Create(Guid.NewGuid(), command.UserId, command.ApplicationId, targetRoles);
            userApp.ApplyLicenseRole(targetRoles, null);
            await _userApplications.AddAsync(userApp, cancellationToken);
        }
        else
        {
            userApp.ApplyLicenseRole(targetRoles, null);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        ISubscriptionPlanRepository planRepository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _planRepository = planRepository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(UpgradeSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (subscription == null)
        {
            throw new BusinessRuleValidationException("NoActiveSubscription", "No active subscription found");
        }

        var newPlan = await _planRepository.GetByIdWithRoleAssignmentsAsync(command.NewSubscriptionPlanId, cancellationToken);
        if (newPlan == null)
            throw new BusinessRuleValidationException("SubscriptionPlanNotFound", $"Subscription plan '{command.NewSubscriptionPlanId}' not found.");
        if (!newPlan.ApplicationId.HasValue || newPlan.ApplicationId.Value != subscription.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "License package must belong to the same application.");

        var newSnapshot = newPlan.CreateEntitlementSnapshot(subscription.MaxMembers);
        subscription.UpgradeSubscription(command.NewType, command.NewSubscriptionPlanId, null, command.ExpiresAt, newSnapshot, _clock.UtcNow);

        await _repository.UpdateAsync(subscription, cancellationToken);

        var roleNames = await _planRepository.GetRoleNamesByIdsAsync(
            newPlan.RoleAssignments.Select(a => a.RoleId),
            cancellationToken);
        var targetRoles = AssignedRolesFormat.Normalize(newPlan.ToAssignedRolesCsv(roleNames));
        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (userApp is null)
        {
            userApp = UserApplication.Create(Guid.NewGuid(), command.UserId, command.ApplicationId, targetRoles);
            userApp.ApplyLicenseRole(targetRoles, null);
            await _userApplications.AddAsync(userApp, cancellationToken);
        }
        else
        {
            userApp.ApplyLicenseRole(targetRoles, null);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class ActivateLicensePackageCommandHandler : IRequestHandler<ActivateLicensePackageCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ILicensePackageRepository _licensePackageRepository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateLicensePackageCommandHandler(
        ISubscriptionRepository repository,
        ILicensePackageRepository licensePackageRepository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _licensePackageRepository = licensePackageRepository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(ActivateLicensePackageCommand command, CancellationToken cancellationToken)
    {
        var activeSubscription = await _repository.GetActiveSubscriptionAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (activeSubscription != null)
        {
            throw new BusinessRuleValidationException("ActiveSubscriptionExists", "User already has an active subscription. Cancel it first.");
        }

        var package = await _licensePackageRepository.GetByIdAsync(command.LicensePackageId, cancellationToken);
        if (package == null)
            throw new BusinessRuleValidationException("LicensePackageNotFound", $"License package '{command.LicensePackageId}' not found.");
        if (package.ApplicationId != command.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "License package must belong to the same application.");

        var subscription = Subscription.AssignLicense(
            Guid.NewGuid(),
            command.UserId,
            command.ApplicationId,
            package,
            command.SubscriptionType,
            _clock.UtcNow,
            maxMembers: command.MaxMembers);

        if (command.ExpiresAt.HasValue)
        {
            subscription.ExtendSubscription(command.ExpiresAt.Value, _clock.UtcNow);
        }

        await _repository.AddAsync(subscription, cancellationToken);

        // Ensure user has the correct application-scoped role(s) based on the activated license package (CSV).
        var targetRoles = AssignedRolesFormat.Normalize(package.AssignedRole);
        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (userApp is null)
        {
            userApp = UserApplication.Create(Guid.NewGuid(), command.UserId, command.ApplicationId, targetRoles, package.Id);
            userApp.ApplyLicenseRole(targetRoles, package.Id);
            await _userApplications.AddAsync(userApp, cancellationToken);
        }
        else
        {
            userApp.ApplyLicenseRole(targetRoles, package.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class UpgradeLicensePackageCommandHandler : IRequestHandler<UpgradeLicensePackageCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ILicensePackageRepository _licensePackageRepository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeLicensePackageCommandHandler(
        ISubscriptionRepository repository,
        ILicensePackageRepository licensePackageRepository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _licensePackageRepository = licensePackageRepository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> Handle(UpgradeLicensePackageCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (subscription == null)
        {
            throw new BusinessRuleValidationException("NoActiveSubscription", "No active subscription found");
        }

        var newPackage = await _licensePackageRepository.GetByIdAsync(command.NewLicensePackageId, cancellationToken);
        if (newPackage == null)
            throw new BusinessRuleValidationException("LicensePackageNotFound", $"License package '{command.NewLicensePackageId}' not found.");
        if (newPackage.ApplicationId != subscription.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "License package must belong to the same application.");

        var newSnapshot = newPackage.CreateEntitlementSnapshot();
        subscription.UpgradeSubscription(command.NewType, null, command.NewLicensePackageId, command.ExpiresAt, newSnapshot, _clock.UtcNow);

        await _repository.UpdateAsync(subscription, cancellationToken);

        // Update application role(s) according to the new license package (CSV).
        var targetRoles = AssignedRolesFormat.Normalize(newPackage.AssignedRole);
        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (userApp is null)
        {
            userApp = UserApplication.Create(Guid.NewGuid(), command.UserId, command.ApplicationId, targetRoles, newPackage.Id);
            userApp.ApplyLicenseRole(targetRoles, newPackage.Id);
            await _userApplications.AddAsync(userApp, cancellationToken);
        }
        else
        {
            userApp.ApplyLicenseRole(targetRoles, newPackage.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class ExtendSubscriptionCommandHandler : IRequestHandler<ExtendSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IClock _clock;

    public ExtendSubscriptionCommandHandler(ISubscriptionRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<SubscriptionDto> Handle(ExtendSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (subscription == null)
        {
            throw new BusinessRuleValidationException("NoActiveSubscription", "No active subscription found");
        }

        subscription.ExtendSubscription(command.NewExpiresAt, _clock.UtcNow);

        await _repository.UpdateAsync(subscription, cancellationToken);

        return SubscriptionEntityMapper.ToDto(subscription);
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Unit>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionCommandHandler(
        ISubscriptionRepository repository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetActiveSubscriptionForUpdateAsync(
            command.UserId,
            command.ApplicationId,
            cancellationToken);

        if (subscription == null)
        {
            throw new BusinessRuleValidationException("NoActiveSubscription", "No active subscription found");
        }

        subscription.CancelSubscription(_clock.UtcNow);
        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, command.ApplicationId, cancellationToken);
        if (userApp is not null)
        {
            userApp.ClearLicenseRole();
        }

        await _repository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class AddSubscriptionMemberCommandHandler : IRequestHandler<AddSubscriptionMemberCommand, Unit>
{
    private readonly ISubscriptionRepository _repository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILicensePackageRepository _licensePackageRepository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public AddSubscriptionMemberCommandHandler(
        ISubscriptionRepository repository,
        ISubscriptionPlanRepository planRepository,
        ILicensePackageRepository licensePackageRepository,
        IUserApplicationRepository userApplications,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _planRepository = planRepository;
        _licensePackageRepository = licensePackageRepository;
        _userApplications = userApplications;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddSubscriptionMemberCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdWithMembersAsync(command.SubscriptionId, cancellationToken);
        if (subscription == null)
            throw new BusinessRuleValidationException("SubscriptionNotFound", "Subscription not found");

        if (subscription.ApplicationId != command.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "ApplicationId does not match this subscription.");

        subscription.AddMember(command.UserId, command.RoleTypeId, _clock.UtcNow);
        
        await _repository.UpdateAsync(subscription, cancellationToken);

        var targetRoles = await ResolveSubscriptionAssignedRolesAsync(subscription, cancellationToken);
        if (!string.IsNullOrWhiteSpace(targetRoles))
        {
            var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, subscription.ApplicationId, cancellationToken);
            if (userApp is null)
            {
                userApp = UserApplication.Create(Guid.NewGuid(), command.UserId, subscription.ApplicationId, targetRoles, subscription.LicensePackageId);
                userApp.ApplyLicenseRole(targetRoles, subscription.LicensePackageId);
                await _userApplications.AddAsync(userApp, cancellationToken);
            }
            else
            {
                userApp.ApplyLicenseRole(targetRoles, subscription.LicensePackageId);
            }
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<string?> ResolveSubscriptionAssignedRolesAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        if (subscription.LicensePackageId.HasValue)
        {
            var package = await _licensePackageRepository.GetByIdAsync(subscription.LicensePackageId.Value, cancellationToken);
            return package is null ? null : AssignedRolesFormat.Normalize(package.AssignedRole);
        }

        if (subscription.SubscriptionPlanId.HasValue)
        {
            var plan = await _planRepository.GetByIdWithRoleAssignmentsAsync(subscription.SubscriptionPlanId.Value, cancellationToken);
            if (plan is null)
                return null;
            var roleNames = await _planRepository.GetRoleNamesByIdsAsync(
                plan.RoleAssignments.Select(a => a.RoleId),
                cancellationToken);
            return AssignedRolesFormat.Normalize(plan.ToAssignedRolesCsv(roleNames));
        }

        return null;
    }
}

public class RemoveSubscriptionMemberCommandHandler : IRequestHandler<RemoveSubscriptionMemberCommand, Unit>
{
    private readonly ISubscriptionRepository _repository;
    private readonly IUserApplicationRepository _userApplications;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSubscriptionMemberCommandHandler(
        ISubscriptionRepository repository,
        IUserApplicationRepository userApplications,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _userApplications = userApplications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveSubscriptionMemberCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdWithMembersAsync(command.SubscriptionId, cancellationToken);
        if (subscription == null)
            throw new BusinessRuleValidationException("SubscriptionNotFound", "Subscription not found");

        if (subscription.ApplicationId != command.ApplicationId)
            throw new BusinessRuleValidationException("ApplicationMismatch", "ApplicationId does not match this subscription.");

        subscription.RemoveMember(command.UserId);
        
        await _repository.UpdateAsync(subscription, cancellationToken);

        var userApp = await _userApplications.GetByUserAndAppAsync(command.UserId, subscription.ApplicationId, cancellationToken);
        if (userApp is not null)
        {
            userApp.ClearLicenseRole();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
