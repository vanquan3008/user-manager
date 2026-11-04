using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Security;

/// <summary>
/// Encrypts and authenticates the pending-MFA state with Data Protection; the expiry is part of the
/// authenticated payload, so the token cannot be extended or forged by the client.
/// </summary>
public sealed class MfaChallengeService : IMfaChallengeService
{
    private readonly IDataProtector _protector;
    private readonly IClock _clock;
    private readonly AppSettings _settings;

    public MfaChallengeService(IDataProtectionProvider provider, IClock clock, AppSettings settings)
    {
        _protector = provider.CreateProtector("UserService.MfaChallenge.v1");
        _clock = clock;
        _settings = settings;
    }

    public (string Token, DateTime ExpiresAt) Create(MfaChallenge challenge)
    {
        var expiresAt = _clock.UtcNow.AddMinutes(Math.Max(1, _settings.Security.MfaChallengeMinutes));
        var payload = JsonSerializer.Serialize(new Envelope(challenge, expiresAt));
        return (_protector.Protect(payload), expiresAt);
    }

    public MfaChallenge? Read(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;
        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(_protector.Unprotect(token.Trim()));
            return envelope is not null && envelope.ExpiresAt > _clock.UtcNow ? envelope.Challenge : null;
        }
        catch
        {
            return null;
        }
    }

    private sealed record Envelope(MfaChallenge Challenge, DateTime ExpiresAt);
}
