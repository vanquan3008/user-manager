using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IAuthServices _authService;

    public LoginUserCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        return await _authService.LoginUserAsync(
            command.Email,
            command.Password,
            command.ApplicationId,
            command.ReturnUrl,
            cancellationToken);
    }
}
