using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;
public interface IMagicLinkService
{
    Task<(string Token, MagicLinkToken TokenEntity)> GenerateMagicLinkTokenWithoutPersistingAsync(
        Guid userId,
        string purpose,
        string? email = null,
        CancellationToken cancellationToken = default);
    Task PersistMagicLinkTokenAsync(MagicLinkToken tokenEntity, CancellationToken cancellationToken = default);
    Task<string> GenerateMagicLinkAsync(
        Guid userId, 
        string purpose, 
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
    Task<(bool IsValid, Guid? UserId)> ValidateMagicLinkAsync(
        string token, 
        string purpose, 
        CancellationToken cancellationToken = default);
    Task InvalidateMagicLinksAsync(Guid userId, string purpose, CancellationToken cancellationToken = default);
    string BuildMagicLinkUrl(string token, string purpose, string baseUrl, string? email = null, string? clientId = null);
}
