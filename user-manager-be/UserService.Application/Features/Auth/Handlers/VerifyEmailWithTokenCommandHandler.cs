using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyEmailWithTokenCommandHandler : IRequestHandler<VerifyEmailWithTokenCommand, bool>
{
    private readonly IAuthServices _authService;

    public VerifyEmailWithTokenCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(VerifyEmailWithTokenCommand command, CancellationToken cancellationToken)
    {
        return await _authService.VerifyEmailWithTokenAsync(command.Token, cancellationToken);
    }
}
