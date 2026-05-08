using FluentValidation;
using UserService.Application.Features.Users.Commands;

namespace UserService.Application.Features.Users.Validators;

/// <summary>
/// Validator for AddToRoleCommand. Role existence is validated in handler (against DB).
/// </summary>
public class AddToRoleCommandValidator : AbstractValidator<AddToRoleCommand>
{
    public AddToRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .NotNull()
            .MaximumLength(256)
            .WithMessage("Role name is too long");
    }
}
