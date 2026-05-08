using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IAuthServices _authService;

    public VerifyEmailCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _authService.VerifyEmailAsync(command.UserId, command.Token, cancellationToken);

        if (!succeeded)
        {
            throw new BusinessRuleValidationException("VerifyEmailFailed", "Failed to verify email. The verification link may have expired.");
        }
        return Unit.Value;
    }
}
