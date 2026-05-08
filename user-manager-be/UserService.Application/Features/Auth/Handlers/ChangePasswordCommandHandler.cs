using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        ILogger<ChangePasswordCommandHandler> logger
    )
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing password for user {UserId}", command.UserId);

        var succeeded = await _identityService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (!succeeded)
        {
            _logger.LogWarning("Failed to change password for user {UserId}", command.UserId);
            throw new BusinessRuleValidationException("ChangePasswordFailed", "Failed to change password. Verify your current password is correct.");
        }

        _logger.LogInformation("Password changed for user {UserId}", command.UserId);
        return Unit.Value;
    }
}
