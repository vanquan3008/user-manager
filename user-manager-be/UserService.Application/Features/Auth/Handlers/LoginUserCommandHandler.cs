using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly ILoginService _loginService;
    private readonly IApplicationAppService _applicationService;

    public LoginUserCommandHandler(ILoginService loginService, IApplicationAppService applicationService)
    {
        _loginService = loginService;
        _applicationService = applicationService;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var applicationId = await _applicationService.ResolveApplicationIdAsync(command.ClientId, cancellationToken);

        var request = new LoginUserRequest(
            command.Email,
            command.Password,
            applicationId,
            command.ReturnUrl);

        return await _loginService.LoginAsync(request, cancellationToken);
    }
}
