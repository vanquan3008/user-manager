using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyEmailWithTokenCommandHandler : IRequestHandler<VerifyEmailWithTokenCommand, bool>
{
    private readonly IEmailVerificationService _emailVerificationService;

    public VerifyEmailWithTokenCommandHandler(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    public async Task<bool> Handle(VerifyEmailWithTokenCommand command, CancellationToken cancellationToken)
    {
        return await _emailVerificationService.VerifyEmailWithTokenAsync(command.Token, cancellationToken);
    }
}
