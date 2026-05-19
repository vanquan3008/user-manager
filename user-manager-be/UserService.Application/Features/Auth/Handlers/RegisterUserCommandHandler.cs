using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IRegistrationService _registrationService;
    private readonly IApplicationAppService _applicationService;

    public RegisterUserCommandHandler(IRegistrationService registrationService, IApplicationAppService applicationService)
    {
        _registrationService = registrationService;
        _applicationService = applicationService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var applicationId = await _applicationService.ResolveApplicationIdAsync(command.ClientId, cancellationToken);

        return await _registrationService.RegisterAsync(
            new RegisterUserRequest(
                command.Email,
                command.Password,
                command.Username,
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                applicationId),
            cancellationToken);
    }
}
