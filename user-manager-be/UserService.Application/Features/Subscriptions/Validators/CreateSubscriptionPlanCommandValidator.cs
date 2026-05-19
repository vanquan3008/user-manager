using FluentValidation;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Features.Subscriptions.Validators;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MonthlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.YearlyPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUsers)
            .Must(v => v == EntitlementQuotas.Unlimited || v >= 0);
        RuleFor(x => x.MaxApiCalls)
            .Must(v => !v.HasValue || v.Value == EntitlementQuotas.Unlimited || v.Value >= 0);
        RuleForEach(x => x.Features)
            .NotEmpty()
            .MaximumLength(100);
        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .MaximumLength(100);
    }
}
