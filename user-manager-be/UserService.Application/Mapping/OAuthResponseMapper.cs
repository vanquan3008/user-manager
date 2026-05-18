using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Mapping;

public sealed record OAuthAuthorizeResponseSource(
    bool RequiresConsent,
    string ClientName,
    string RedirectUri,
    string[] RequestedScopes,
    string[] AlreadyGrantedScopes,
    string[] MissingScopes,
    string? Code,
    string? State);

public static class OAuthResponseMapper
{

    public static OAuthAuthorizeResponse ToAuthorizeResponse(OAuthAuthorizeResponseSource src) =>
        new OAuthAuthorizeResponse(
            src.RequiresConsent,
            src.ClientName,
            src.RedirectUri,
            src.RequestedScopes,
            src.AlreadyGrantedScopes,
            src.MissingScopes,
            src.Code,
            src.State);
}
