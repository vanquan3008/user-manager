using UserService.Application.DTOs.Auth;
using UserService.Application.Interfaces.Services;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using static UserService.Domain.Constants.EmailPurpose.MagicLinkPurpose;

namespace UserService.Application.Services.Auth;

public sealed class MagicLinkAuthService : IMagicLinkAuthService
{
    private readonly IMagicLinkService _magicLinkService;
    private readonly IIdentityService _identityService;
    private readonly AuthResponseFactory _authResponseFactory;
    private readonly IAuditLogger _audit;

    public MagicLinkAuthService(
        IMagicLinkService magicLinkService,
        IIdentityService identityService,
        AuthResponseFactory authResponseFactory,
        IAuditLogger audit)
    {
        _magicLinkService = magicLinkService;
        _identityService = identityService;
        _authResponseFactory = authResponseFactory;
        _audit = audit;
    }

    public async Task<AuthResponse> VerifyMagicLinkAndAuthenticateAsync(
        VerifyMagicLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        // Only passwordless-login links may sign a user in. Previously any purpose (e.g. email
        // verification or password reset links) was accepted here.
        if (!string.Equals(request.Purpose, PasswordlessLogin, StringComparison.OrdinalIgnoreCase))
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "Invalid or expired magic link.");

        var (isValid, userId) = await _magicLinkService.ValidateMagicLinkAsync(request.Token, PasswordlessLogin, cancellationToken);
        if (!isValid || !userId.HasValue)
        {
            await _audit.LogAsync(AuditEventTypes.MagicLinkLogin, false, new AuditEntry(Reason: "invalid_or_expired"), cancellationToken);
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidCredentials, "Invalid or expired magic link.");
        }

        var user = await _identityService.GetUserByIdAsync(userId.Value, cancellationToken);
        if (user is null || !await _identityService.IsSignInAllowedAsync(user.Id, cancellationToken))
            throw new AuthenticationFlowException(AuthErrorCodes.AccountDisabled, "This account is disabled or locked.");

        return await _authResponseFactory.CompleteConsoleSignInAsync(user, request.ApplicationId, AuthenticationMethods.MagicLink, cancellationToken);
    }
}
