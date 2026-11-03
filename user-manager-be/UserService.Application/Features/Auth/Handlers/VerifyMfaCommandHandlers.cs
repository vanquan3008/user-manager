using MediatR;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Interfaces.Services.Security;
using UserService.Application.Services.Auth;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Features.Auth.Handlers;

internal static class MfaChallengeVerifier
{
    public static async Task<MfaChallenge> VerifyAsync(
        IMfaChallengeService challenges,
        IMfaService mfa,
        IAuditLogger audit,
        string token,
        string code,
        string expectedKind,
        CancellationToken cancellationToken)
    {
        var challenge = challenges.Read(token);
        if (challenge is null || challenge.SessionKind != expectedKind)
            throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaChallenge, "The sign-in attempt has expired. Please sign in again.");

        var result = await mfa.VerifyAsync(challenge.UserId, code, cancellationToken);
        switch (result)
        {
            case MfaVerificationResult.Succeeded:
                await audit.LogAsync(AuditEventTypes.MfaVerified, true, new AuditEntry(ActorUserId: challenge.UserId), cancellationToken);
                return challenge;
            case MfaVerificationResult.LockedOut:
                await audit.LogAsync(AuditEventTypes.MfaFailed, false, new AuditEntry(ActorUserId: challenge.UserId, Reason: AuthErrorCodes.AccountLocked), cancellationToken);
                throw new AuthenticationFlowException(AuthErrorCodes.AccountLocked, "Too many failed attempts. The account is temporarily locked.");
            case MfaVerificationResult.NotEnabled:
                throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaChallenge, "The sign-in attempt has expired. Please sign in again.");
            default:
                await audit.LogAsync(AuditEventTypes.MfaFailed, false, new AuditEntry(ActorUserId: challenge.UserId, Reason: AuthErrorCodes.InvalidMfaCode), cancellationToken);
                throw new AuthenticationFlowException(AuthErrorCodes.InvalidMfaCode, "The verification code is invalid or expired.");
        }
    }
}

public sealed class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, AuthResponse>
{
    private readonly IMfaChallengeService _challenges;
    private readonly IMfaService _mfa;
    private readonly IAuditLogger _audit;
    private readonly AuthResponseFactory _responses;

    public VerifyMfaCommandHandler(IMfaChallengeService challenges, IMfaService mfa, IAuditLogger audit, AuthResponseFactory responses)
    {
        _challenges = challenges;
        _mfa = mfa;
        _audit = audit;
        _responses = responses;
    }

    public async Task<AuthResponse> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var challenge = await MfaChallengeVerifier.VerifyAsync(_challenges, _mfa, _audit, request.MfaToken, request.Code, UserSessionKinds.Console, cancellationToken);
        return await _responses.StartConsoleSessionAsync(challenge.UserId, challenge.ApplicationId, AuthenticationMethods.Mfa, cancellationToken);
    }
}

public sealed class VerifySsoMfaCommandHandler : IRequestHandler<VerifySsoMfaCommand, SsoLoginResponse>
{
    private readonly IMfaChallengeService _challenges;
    private readonly IMfaService _mfa;
    private readonly IAuditLogger _audit;
    private readonly AuthResponseFactory _responses;

    public VerifySsoMfaCommandHandler(IMfaChallengeService challenges, IMfaService mfa, IAuditLogger audit, AuthResponseFactory responses)
    {
        _challenges = challenges;
        _mfa = mfa;
        _audit = audit;
        _responses = responses;
    }

    public async Task<SsoLoginResponse> Handle(VerifySsoMfaCommand request, CancellationToken cancellationToken)
    {
        var challenge = await MfaChallengeVerifier.VerifyAsync(_challenges, _mfa, _audit, request.MfaToken, request.Code, UserSessionKinds.HostedSso, cancellationToken);
        return await _responses.StartHostedSsoSessionAsync(challenge.UserId, AuthenticationMethods.Mfa, cancellationToken);
    }
}
