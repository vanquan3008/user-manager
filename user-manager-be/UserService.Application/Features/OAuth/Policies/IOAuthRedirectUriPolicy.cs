namespace UserService.Application.Features.OAuth.Policies;

public interface IOAuthRedirectUriPolicy
{
    bool IsAllowed(string redirectUri, string? appAllowedUris = null);
}

