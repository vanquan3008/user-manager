using FluentValidation;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Services;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Subscriptions.Validators;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator(IPlanApplicationRoleValidator planRoleValidator)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MonthlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.YearlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers)
            .Must(v => v == EntitlementQuotas.Unlimited || v >= 0);
        RuleFor(x => x.MaxApiCalls)
            .Must(v => !v.HasValue || v.Value == EntitlementQuotas.Unlimited || v.Value >= 0);

        When(x => x.ApplicationId.HasValue, () =>
        {
            RuleFor(x => x.AssignedRoleIds).NotEmpty();
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    await planRoleValidator.AllRoleIdsLinkedToApplicationAsync(cmd.ApplicationId!.Value, cmd.AssignedRoleIds, ct))
                .WithMessage("Each role id must be linked to this application (ApplicationRoles).");
        });

        When(x => !x.ApplicationId.HasValue, () =>
        {
            RuleFor(x => x.AssignedRoleIds).Empty();
        });
    }
}
