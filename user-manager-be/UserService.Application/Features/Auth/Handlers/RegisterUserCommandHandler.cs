using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IAuthServices _authService;

    public RegisterUserCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        return await _authService.RegisterUserAsync(
            command.Email,
            command.Password,
            command.Username,
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.ApplicationId,
            cancellationToken);
    }
}
