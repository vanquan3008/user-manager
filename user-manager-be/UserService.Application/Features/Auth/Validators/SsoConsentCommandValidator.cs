using FluentValidation;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Features.Auth.Validators;

public class SsoConsentCommandValidator : AbstractValidator<SsoConsentCommand>
{
    public SsoConsentCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("RedirectUri is required")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("RedirectUri must be a valid absolute URI");

        RuleFor(x => x.Scopes)
            .NotEmpty().WithMessage("At least one scope is required");

        RuleFor(x => x.CodeChallenge)
            .NotEmpty().WithMessage("CodeChallenge is required");

        RuleFor(x => x.CodeChallengeMethod)
            .Must(method => string.IsNullOrWhiteSpace(method) || string.Equals(method, "S256", StringComparison.OrdinalIgnoreCase))
            .WithMessage("CodeChallengeMethod must be 'S256'");
    }
}
