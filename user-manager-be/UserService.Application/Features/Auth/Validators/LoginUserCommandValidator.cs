using FluentValidation;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Features.Auth.Validators;

/// <summary>
/// Validator for LoginUserCommand
/// </summary>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
