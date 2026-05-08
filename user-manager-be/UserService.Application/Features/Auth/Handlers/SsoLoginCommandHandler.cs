using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class SsoLoginCommandHandler : IRequestHandler<SsoLoginCommand, AuthResponse>
{
    private readonly IAuthServices _authService;

    public SsoLoginCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(SsoLoginCommand command, CancellationToken cancellationToken)
    {
        return await _authService.SsoLoginAsync(command.Email, command.Password, cancellationToken);
    }
}

