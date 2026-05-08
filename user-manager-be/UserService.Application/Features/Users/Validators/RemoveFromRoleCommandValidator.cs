using FluentValidation;
using UserService.Application.Features.Users.Commands;

namespace UserService.Application.Features.Users.Validators;

/// <summary>
/// Validator for RemoveFromRoleCommand
/// </summary>
public class RemoveFromRoleCommandValidator : AbstractValidator<RemoveFromRoleCommand>
{
    public RemoveFromRoleCommandValidator()
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
