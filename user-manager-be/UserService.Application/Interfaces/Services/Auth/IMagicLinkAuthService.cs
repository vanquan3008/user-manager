using UserService.Application.DTOs.Auth;

namespace UserService.Application.Interfaces.Services;

public interface IMagicLinkAuthService
{
    Task<AuthResponse> VerifyMagicLinkAndAuthenticateAsync(
        VerifyMagicLinkRequest request,
        CancellationToken cancellationToken = default);
}
