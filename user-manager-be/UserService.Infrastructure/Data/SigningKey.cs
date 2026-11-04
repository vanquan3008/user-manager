namespace UserService.Infrastructure.Data;

/// <summary>
/// Asymmetric token signing key (RS256). The private key is encrypted with ASP.NET Core Data Protection
/// before it is persisted. Retired keys stay published in the JWKS for a retention window so tokens
/// signed before a rotation keep validating.
/// </summary>
public sealed class SigningKey
{
    public Guid Id { get; set; }
    public string KeyId { get; set; } = string.Empty;
    public string Algorithm { get; set; } = "RS256";
    public int KeySize { get; set; }
    public string PublicKeyPem { get; set; } = string.Empty;
    public string ProtectedPrivateKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RetiredAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RetiredAt is null && RevokedAt is null;
}
