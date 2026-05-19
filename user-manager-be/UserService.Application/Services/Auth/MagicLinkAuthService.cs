using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Exceptions;

namespace UserService.Application.Services.Auth;

public sealed class MagicLinkAuthService : IMagicLinkAuthService
{
    private readonly IMagicLinkService _magicLinkService;
    private readonly AuthResponseFactory _authResponseFactory;

    public MagicLinkAuthService(
        IMagicLinkService magicLinkService,
        AuthResponseFactory authResponseFactory)
    {
        _magicLinkService = magicLinkService;
        _authResponseFactory = authResponseFactory;
    }

    public async Task<AuthResponse> VerifyMagicLinkAndAuthenticateAsync(
        VerifyMagicLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(
            request.Token,
            request.Purpose,
            cancellationToken);

        if (!isValid || !userId.HasValue)
            throw new UnauthorizedException("Invalid or expired magic link");

        return await _authResponseFactory.BuildUserAuthResponseAsync(
            userId.Value,
            request.ApplicationId,
            cancellationToken);
    }
}
