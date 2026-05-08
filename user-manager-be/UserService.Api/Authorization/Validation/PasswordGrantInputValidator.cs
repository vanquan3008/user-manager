using FluentValidation;

namespace UserService.Api.Authorization.Validation;

public sealed class PasswordGrantInputValidator : AbstractValidator<PasswordGrantInput>
{
    public PasswordGrantInputValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}

