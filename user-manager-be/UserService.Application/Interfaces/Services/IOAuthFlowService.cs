using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Interfaces.Services;

public interface IOAuthFlowService
{
    Task<OAuthAuthorizeResponse> AuthorizeAsync(Guid userId, OAuthAuthorizeRequest request, CancellationToken cancellationToken = default);
    Task<OAuthAuthorizeResponse> ConsentAsync(Guid userId, OAuthConsentRequest request, CancellationToken cancellationToken = default);
    Task<OAuthTokenResponse> ExchangeTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default);
    Task<OAuthUserInfoDto> GetUserInfoAsync(Guid userId, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken = default);
    /// <summary>RFC 7009 token revocation for refresh tokens. Always succeeds from the caller's perspective.</summary>
    Task RevokeTokenAsync(string clientId, string? clientSecret, string token, CancellationToken cancellationToken = default);
}
