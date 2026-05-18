using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Interfaces.Services;

public interface ISSOAuthenticationService
{
    Task<AuthResponse> SSOLoginAsync(SSOLoginRequest request, CancellationToken cancellationToken = default);
    Task<SSOAuthorizeResponse> AuthorizeAsync(SSOAuthorizeCommand command, CancellationToken cancellationToken = default);
    Task<SSOAuthorizeResponse> ConsentAsync(SSOConsentCommand command, CancellationToken cancellationToken = default);
    Task<AuthResponse> ExchangeTokenAsync(SSOExchangeTokenCommand command, CancellationToken cancellationToken = default);
}
