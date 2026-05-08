using FluentValidation;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Services;
using UserService.Domain;
using UserService.Domain.Repositories;

namespace UserService.Application.Features.Subscriptions.Validators;

public sealed class CreateLicensePackageCommandValidator : AbstractValidator<CreateLicensePackageCommand>
{
    public CreateLicensePackageCommandValidator(
        ISubscriptionPlanRepository planRepository,
        IApplicationRoleService roleService)
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.SubscriptionPlanId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.SeatLimit).GreaterThan(0);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var plan = await planRepository.GetByIdAsync(cmd.SubscriptionPlanId, ct);
                if (plan is null) return false;
                if (plan.ApplicationId != cmd.ApplicationId) return false;
                foreach (var roleName in AssignedRolesFormat.Split(cmd.AssignedRole))
                {
                    if (!await roleService.RoleExistsForApplicationAsync(cmd.ApplicationId, roleName, ct))
                        return false;
                }
                return true;
            })
            .WithMessage("SubscriptionPlan not found / application mismatch / assigned role not found.");
    }
}

