namespace UserService.Application.Interfaces.Services.Security;

public interface ITotpService
{
    /// <summary>
    /// Validates a 6-digit RFC 6238 code (SHA-1, 30 s step, ±1 step drift).
    /// Codes whose time-step is at or before <paramref name="lastUsedTimeStep"/> are rejected to prevent replay.
    /// </summary>
    bool TryValidate(string base32Secret, string code, DateTime utcNow, long? lastUsedTimeStep, out long matchedTimeStep);
    string ComputeCode(byte[] secret, long timeStep);
    string BuildAuthenticatorUri(string issuer, string accountName, string base32Secret);
}
