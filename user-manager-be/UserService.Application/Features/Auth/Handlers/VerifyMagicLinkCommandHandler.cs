using MediatR;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;
using UserService.Application.Services;

namespace UserService.Application.Features.Auth.Handlers;

public class VerifyMagicLinkCommandHandler : IRequestHandler<VerifyMagicLinkCommand, AuthResponse>
{
    private readonly IAuthServices _authService;

    public VerifyMagicLinkCommandHandler(IAuthServices authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(VerifyMagicLinkCommand command, CancellationToken cancellationToken)
    {
        return await _authService.VerifyMagicLinkAndAuthenticateAsync(
            command.Token,
            command.Purpose,
            command.ApplicationId,
            cancellationToken);
    }
}
