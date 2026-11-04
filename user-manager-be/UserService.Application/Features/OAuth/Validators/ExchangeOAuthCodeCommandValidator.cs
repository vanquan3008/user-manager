using FluentValidation;
using UserService.Application.Features.OAuth.Commands;

namespace UserService.Application.Features.OAuth.Validators;

public sealed class ExchangeOAuthCodeCommandValidator : AbstractValidator<ExchangeOAuthCodeCommand>
{
    public ExchangeOAuthCodeCommandValidator()
    {
        RuleFor(x => x.Request.ClientId).NotEmpty();
        // Unsupported values are rejected by the flow service with unsupported_grant_type (RFC 6749 §5.2).
        RuleFor(x => x.Request.GrantType).NotEmpty().WithMessage("grant_type is required.");

        When(x => string.Equals(x.Request.GrantType, "authorization_code", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Request.Code).NotEmpty().WithMessage("Missing code.");
            RuleFor(x => x.Request.RedirectUri).NotEmpty().WithMessage("redirect_uri is required.");
            RuleFor(x => x.Request.CodeVerifier).NotEmpty().WithMessage("Missing code_verifier.");
        });

        When(x => string.Equals(x.Request.GrantType, "refresh_token", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Request.RefreshToken).NotEmpty().WithMessage("Missing refresh_token.");
        });
    }
}

