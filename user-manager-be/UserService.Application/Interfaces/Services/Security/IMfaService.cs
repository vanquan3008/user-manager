namespace UserService.Application.Interfaces.Services.Security;

public sealed record MfaStatus(bool Enabled, DateTime? EnabledAt, int RecoveryCodesRemaining);
public sealed record MfaSetup(string SharedKey, string AuthenticatorUri);

public enum MfaVerificationResult
{
    Succeeded,
    Invalid,
    LockedOut,
    NotEnabled
}

public interface IMfaService
{
    Task<MfaStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MfaSetup> BeginSetupAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Confirms setup with a TOTP code and returns one-time recovery codes.</summary>
    Task<IReadOnlyList<string>> EnableAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task DisableAsync(Guid userId, string password, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    /// <summary>Administrative reset for users who lost their authenticator (requires re-enrolment).</summary>
    Task AdminResetAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Verifies a TOTP or recovery code, enforcing lockout and replay protection.</summary>
    Task<MfaVerificationResult> VerifyAsync(Guid userId, string code, CancellationToken cancellationToken = default);
}

public sealed record MfaChallenge(Guid UserId, string SessionKind, string AuthenticationMethod, Guid? ApplicationId);

/// <summary>Issues short-lived, tamper-proof tokens that carry a pending (password-verified) login to the MFA step.</summary>
public interface IMfaChallengeService
{
    (string Token, DateTime ExpiresAt) Create(MfaChallenge challenge);
    MfaChallenge? Read(string token);
}
