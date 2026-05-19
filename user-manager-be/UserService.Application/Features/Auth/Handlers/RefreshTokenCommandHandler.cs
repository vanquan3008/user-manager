using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly ILoginService _loginService;

    public RefreshTokenCommandHandler(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        return await _loginService.RefreshTokenAsync(command.RefreshToken, cancellationToken);
    }
}
