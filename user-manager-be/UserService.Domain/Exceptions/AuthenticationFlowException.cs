namespace UserService.Domain.Exceptions;

/// <summary>
/// Authentication failure with a stable, client-visible error code (e.g. <c>INVALID_CREDENTIALS</c>,
/// <c>MFA_REQUIRED</c>, <c>REFRESH_TOKEN_REUSED</c>). Messages must never reveal whether an account exists.
/// </summary>
public sealed class AuthenticationFlowException : DomainException
{
    public string Code { get; }

    public AuthenticationFlowException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public static class AuthErrorCodes
{
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string AccountDisabled = "ACCOUNT_DISABLED";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string RefreshTokenSuperseded = "REFRESH_TOKEN_SUPERSEDED";
    public const string RefreshTokenReused = "REFRESH_TOKEN_REUSED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string InvalidMfaChallenge = "INVALID_MFA_CHALLENGE";
    public const string InvalidMfaCode = "INVALID_MFA_CODE";
    public const string MfaNotEnabled = "MFA_NOT_ENABLED";
    public const string WrongLoginEndpoint = "WRONG_LOGIN_ENDPOINT";
}

/// <summary>Raised when an optimistic concurrency check fails (e.g. a one-time token consumed twice concurrently).</summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string message, Exception? inner = null)
        : base(message, inner ?? new Exception(message))
    {
    }
}
