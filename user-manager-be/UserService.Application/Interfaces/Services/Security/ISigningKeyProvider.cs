using Microsoft.IdentityModel.Tokens;

namespace UserService.Application.Interfaces.Services.Security;

public sealed record JsonWebKeyDto(string Kty, string Use, string Alg, string Kid, string N, string E);

public sealed record SigningKeyInfo(
    string KeyId,
    string Algorithm,
    int KeySize,
    string Status,
    DateTime CreatedAt,
    DateTime? RetiredAt,
    DateTime? RevokedAt,
    DateTime? PublishedUntil);

/// <summary>Asymmetric signing key ring with rotation; public keys are published as a JWKS.</summary>
public interface ISigningKeyProvider
{
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
    /// <summary>Reloads keys from storage and performs scheduled rotation when due.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
    SigningCredentials GetSigningCredentials();
    IReadOnlyCollection<SecurityKey> GetValidationKeys();
    IReadOnlyList<JsonWebKeyDto> GetPublicJwks();
    Task<IReadOnlyList<SigningKeyInfo>> ListAsync(CancellationToken cancellationToken = default);
    Task<SigningKeyInfo> RotateAsync(CancellationToken cancellationToken = default);
    Task RevokeAsync(string keyId, CancellationToken cancellationToken = default);
}
