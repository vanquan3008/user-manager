using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Interfaces.Services;

public interface IApplicationTokenExchangeService
{
    Task<AuthResponse> ExchangeTokenAsync(ExchangeApplicationTokenCommand command, CancellationToken cancellationToken = default);
}
