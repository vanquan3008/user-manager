using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Features.Auth.Handlers;

public sealed class ExchangeApplicationTokenCommandHandler : IRequestHandler<ExchangeApplicationTokenCommand, AuthResponse>
{
    private readonly IApplicationTokenExchangeService _applicationTokenExchangeService;

    public ExchangeApplicationTokenCommandHandler(IApplicationTokenExchangeService applicationTokenExchangeService)
    {
        _applicationTokenExchangeService = applicationTokenExchangeService;
    }

    public Task<AuthResponse> Handle(ExchangeApplicationTokenCommand command, CancellationToken cancellationToken)
        => _applicationTokenExchangeService.ExchangeTokenAsync(command, cancellationToken);
}
