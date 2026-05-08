using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    private readonly IAuthServices _authService;

    public RequestPasswordResetCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _authService.RequestPasswordResetAsync(command.Email, cancellationToken);

        if (!succeeded)
        {
            throw new BusinessRuleValidationException("RequestPasswordResetFailed", "Failed to send password reset link");
        }

        return Unit.Value;
    }
}
