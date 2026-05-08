using FluentValidation;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Features.Auth.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .NotEqual(Guid.Empty).WithMessage("UserId is invalid");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required")
            .MinimumLength(8).WithMessage("Current password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Current password must not exceed 100 characters");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters")
            .MaximumLength(100).WithMessage("New password must not exceed 100 characters")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit")
            .Matches("[\\!\\?\\*\\.\\@\\#\\$\\%\\^\\&\\+\\=]").WithMessage("New password must contain at least one special character");

        RuleFor(x => x)
            .Must(cmd => cmd.NewPassword != cmd.CurrentPassword)
            .WithMessage("New password must be different from the current password");
    }
}
