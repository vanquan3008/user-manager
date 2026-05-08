using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.ResetPasswordAsync(
            command.Email,
            command.Token,
            command.NewPassword,
            cancellationToken);

        if (!succeeded)
        {
            throw new BusinessRuleValidationException("ResetPasswordFailed", "Failed to reset password. The reset link may have expired.");
        }

        return Unit.Value;
    }
}
