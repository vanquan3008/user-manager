using FluentValidation;
using UserService.Application.Features.OAuth.Commands;

namespace UserService.Application.Features.OAuth.Validators;

public sealed class ConsentOAuthCommandValidator : AbstractValidator<ConsentOAuthCommand>
{
    public ConsentOAuthCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Request.ClientId).NotEmpty();
        RuleFor(x => x.Request.RedirectUri).NotEmpty().WithMessage("redirect_uri is required.");
        RuleFor(x => x.Request.CodeChallengeMethod)
            .NotEmpty()
            .Must(m => string.Equals(m, "S256", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only PKCE S256 is supported.");
        RuleFor(x => x.Request.CodeChallenge).NotEmpty().WithMessage("Missing code_challenge.");
        RuleFor(x => x.Request.Scopes)
            .Must(scopes => scopes == null || scopes.Any(s => !string.IsNullOrWhiteSpace(s)))
            .WithMessage("Scopes must not be empty when provided.");
    }
}

