using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Common;
using UserService.Application.Configuration;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Security;

/// <summary>
/// Database-backed RS256 key ring. Private keys are encrypted with ASP.NET Core Data Protection.
/// Keys are cached in memory (singleton) and reloaded periodically so all instances converge after a rotation.
/// </summary>
public sealed class SigningKeyProvider : ISigningKeyProvider
{
    private const string ProtectorPurpose = "UserService.SigningKeys.v1";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDataProtector _protector;
    private readonly IClock _clock;
    private readonly SigningKeyConfiguration _config;
    private readonly ILogger<SigningKeyProvider> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private volatile KeyRing _ring = KeyRing.Empty;

    public SigningKeyProvider(
        IServiceScopeFactory scopeFactory,
        IDataProtectionProvider dataProtection,
        IClock clock,
        AppSettings settings,
        ILogger<SigningKeyProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _protector = dataProtection.CreateProtector(ProtectorPurpose);
        _clock = clock;
        _config = settings.Security.SigningKeys;
        _logger = logger;
    }

    public Task EnsureInitializedAsync(CancellationToken cancellationToken = default) => RefreshAsync(cancellationToken);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var now = _clock.UtcNow;

            var active = await db.SigningKeys
                .Where(k => k.RetiredAt == null && k.RevokedAt == null)
                .OrderByDescending(k => k.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var rotationDue = active is not null && _config.RotationDays > 0 &&
                              now - active.CreatedAt >= TimeSpan.FromDays(_config.RotationDays);

            if (active is null || rotationDue)
            {
                await CreateAndActivateAsync(db, now, cancellationToken);
                if (rotationDue)
                    _logger.LogInformation("Signing key {KeyId} reached its rotation age and was retired.", active!.KeyId);
            }

            _ring = await LoadAsync(db, now, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public SigningCredentials GetSigningCredentials()
        => _ring.Signing ?? throw new InvalidOperationException("Signing keys are not initialized. Call EnsureInitializedAsync at startup.");

    public IReadOnlyCollection<SecurityKey> GetValidationKeys() => _ring.ValidationKeys;

    public IReadOnlyList<JsonWebKeyDto> GetPublicJwks() => _ring.Jwks;

    public async Task<IReadOnlyList<SigningKeyInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var now = _clock.UtcNow;
        var keys = await db.SigningKeys.AsNoTracking().OrderByDescending(k => k.CreatedAt).Take(50).ToListAsync(cancellationToken);
        return keys.Select(k => ToInfo(k, now)).ToList();
    }

    public async Task<SigningKeyInfo> RotateAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var now = _clock.UtcNow;
            var created = await CreateAndActivateAsync(db, now, cancellationToken);
            _ring = await LoadAsync(db, now, cancellationToken);
            return ToInfo(created, now);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RevokeAsync(string keyId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var now = _clock.UtcNow;
            var key = await db.SigningKeys.FirstOrDefaultAsync(k => k.KeyId == keyId, cancellationToken)
                      ?? throw new KeyNotFoundException($"Signing key '{keyId}' was not found.");
            if (key.RevokedAt is not null)
                return;

            var wasActive = key.IsActive;
            key.RevokedAt = now;
            key.RetiredAt ??= now;
            await db.SaveChangesAsync(cancellationToken);

            // Never leave the issuer without an active key.
            if (wasActive)
                await CreateAndActivateAsync(db, now, cancellationToken);

            _ring = await LoadAsync(db, now, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<SigningKey> CreateAndActivateAsync(UserDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var current = await db.SigningKeys.Where(k => k.RetiredAt == null && k.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var key in current)
            key.RetiredAt = now;

        using var rsa = RSA.Create(Math.Max(2048, _config.KeySize));
        var entity = new SigningKey
        {
            Id = Guid.NewGuid(),
            KeyId = SecureTokens.Create(12),
            Algorithm = SecurityAlgorithms.RsaSha256,
            KeySize = rsa.KeySize,
            PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem(),
            ProtectedPrivateKey = Convert.ToBase64String(_protector.Protect(rsa.ExportPkcs8PrivateKey())),
            CreatedAt = now
        };

        db.SigningKeys.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created and activated signing key {KeyId}", entity.KeyId);
        return entity;
    }

    private async Task<KeyRing> LoadAsync(UserDbContext db, DateTime now, CancellationToken cancellationToken)
    {
        var retentionCutoff = now - TimeSpan.FromDays(Math.Max(1, _config.RetiredKeyRetentionDays));
        var keys = await db.SigningKeys.AsNoTracking()
            .Where(k => k.RevokedAt == null && (k.RetiredAt == null || k.RetiredAt > retentionCutoff))
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);

        SigningCredentials? signing = null;
        var validation = new List<SecurityKey>();
        var jwks = new List<JsonWebKeyDto>();

        foreach (var key in keys)
        {
            var publicRsa = RSA.Create();
            publicRsa.ImportFromPem(key.PublicKeyPem);
            var parameters = publicRsa.ExportParameters(false);
            validation.Add(new RsaSecurityKey(publicRsa) { KeyId = key.KeyId });
            jwks.Add(new JsonWebKeyDto(
                "RSA", "sig", key.Algorithm, key.KeyId,
                Base64UrlEncoder.Encode(parameters.Modulus!),
                Base64UrlEncoder.Encode(parameters.Exponent!)));

            if (signing is null && key.IsActive)
            {
                try
                {
                    var privateRsa = RSA.Create();
                    privateRsa.ImportPkcs8PrivateKey(_protector.Unprotect(Convert.FromBase64String(key.ProtectedPrivateKey)), out _);
                    signing = new SigningCredentials(new RsaSecurityKey(privateRsa) { KeyId = key.KeyId }, SecurityAlgorithms.RsaSha256);
                }
                catch (CryptographicException ex)
                {
                    _logger.LogCritical(ex,
                        "Cannot decrypt signing key {KeyId}. The Data Protection key ring is missing or changed; rotate the signing key.",
                        key.KeyId);
                }
            }
        }

        return new KeyRing(signing, validation, jwks);
    }

    private SigningKeyInfo ToInfo(SigningKey key, DateTime now)
    {
        var status = key.RevokedAt is not null ? "revoked" : key.RetiredAt is null ? "active" : "retired";
        DateTime? publishedUntil = key.RevokedAt is not null
            ? key.RevokedAt
            : key.RetiredAt?.AddDays(Math.Max(1, _config.RetiredKeyRetentionDays));
        return new SigningKeyInfo(key.KeyId, key.Algorithm, key.KeySize, status, key.CreatedAt, key.RetiredAt, key.RevokedAt, publishedUntil);
    }

    private sealed record KeyRing(SigningCredentials? Signing, IReadOnlyCollection<SecurityKey> ValidationKeys, IReadOnlyList<JsonWebKeyDto> Jwks)
    {
        public static readonly KeyRing Empty = new(null, Array.Empty<SecurityKey>(), Array.Empty<JsonWebKeyDto>());
    }
}
