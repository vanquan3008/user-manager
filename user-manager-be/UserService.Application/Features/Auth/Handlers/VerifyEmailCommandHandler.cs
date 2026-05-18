using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public VerifyEmailCommandHandler(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public async Task<Unit> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _emailVerificationService.VerifyEmailAsync(command.UserId, command.Token, cancellationToken);

        if (!succeeded)
        {
            throw new BusinessRuleValidationException("VerifyEmailFailed", "Failed to verify email. The verification link may have expired.");
        }
        return Unit.Value;
    }
}
