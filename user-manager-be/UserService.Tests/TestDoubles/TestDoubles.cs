using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Interfaces;

namespace UserService.Tests.TestDoubles;

/// <summary>In-memory RS256 key ring supporting rotation, for unit tests.</summary>
public sealed class TestSigningKeyProvider : ISigningKeyProvider
{
    private readonly List<RsaSecurityKey> _keys = new();

    public TestSigningKeyProvider() => AddKey();

    public RsaSecurityKey Active => _keys[^1];

    public RsaSecurityKey AddKey()
    {
        var key = new RsaSecurityKey(RSA.Create(2048)) { KeyId = Guid.NewGuid().ToString("N")[..12] };
        _keys.Add(key);
        return key;
    }

    public void RemoveKey(RsaSecurityKey key) => _keys.Remove(key);

    public Task EnsureInitializedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public SigningCredentials GetSigningCredentials() => new(Active, SecurityAlgorithms.RsaSha256);
    public IReadOnlyCollection<SecurityKey> GetValidationKeys() => _keys.Cast<SecurityKey>().ToList();
    public IReadOnlyList<JsonWebKeyDto> GetPublicJwks() => Array.Empty<JsonWebKeyDto>();
    public Task<IReadOnlyList<SigningKeyInfo>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SigningKeyInfo>>(Array.Empty<SigningKeyInfo>());
    public Task<SigningKeyInfo> RotateAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public Task RevokeAsync(string keyId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}

public sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}

public sealed class FakeRequestContext : IRequestContext
{
    public string? IpAddress { get; set; } = "203.0.113.10";
    public string? UserAgent { get; set; } = "unit-test";
    public string? CorrelationId { get; set; } = "corr-1";
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public Guid? SessionId { get; set; }
}
