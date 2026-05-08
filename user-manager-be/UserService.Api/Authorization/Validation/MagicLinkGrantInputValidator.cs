using FluentValidation;

namespace UserService.Api.Authorization.Validation;

public sealed class MagicLinkGrantInputValidator : AbstractValidator<MagicLinkGrantInput>
{
    public MagicLinkGrantInputValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Magic link token is required.");

        RuleFor(x => x.ApplicationId)
            .Must(value => string.IsNullOrWhiteSpace(value) || Guid.TryParse(value, out _))
            .WithMessage("Application_id must be a valid GUID.");
    }
}

