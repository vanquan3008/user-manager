using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

public class RequestPasswordlessLoginCommandHandler : IRequestHandler<RequestPasswordlessLoginCommand, Unit>
{
    private readonly IAuthServices _authService;

    public RequestPasswordlessLoginCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(RequestPasswordlessLoginCommand command, CancellationToken cancellationToken)
    {
        var succeeded = await _authService.RequestPasswordlessLoginAsync(
            command.Email,
            command.ApplicationId,
            cancellationToken);
        
        if (!succeeded)
        {
            throw new BusinessRuleValidationException("RequestPasswordlessLoginFailed", "Failed to send passwordless login link");
        }

        return Unit.Value;
    }
}
