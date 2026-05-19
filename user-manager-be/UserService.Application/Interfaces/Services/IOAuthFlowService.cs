using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Interfaces.Services;

public interface IOAuthFlowService
{
    Task<OAuthAuthorizeResponse> AuthorizeAsync(Guid userId, OAuthAuthorizeRequest request, CancellationToken cancellationToken = default);
    Task<OAuthAuthorizeResponse> ConsentAsync(Guid userId, OAuthConsentRequest request, CancellationToken cancellationToken = default);
    Task<OAuthTokenResponse> ExchangeTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default);
}
