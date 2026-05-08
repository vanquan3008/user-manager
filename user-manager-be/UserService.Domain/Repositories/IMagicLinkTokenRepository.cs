using UserService.Domain.Entities;
namespace UserService.Domain.Repositories;
public interface IMagicLinkTokenRepository : IRepository<MagicLinkToken>
{
    Task<MagicLinkToken?> GetValidTokenAsync(string token, string purpose, CancellationToken cancellationToken = default);
    Task InvalidateAllTokensAsync(Guid userId, string purpose, CancellationToken cancellationToken = default);
    Task<MagicLinkToken?> GetLatestTokenAsync(Guid userId, string purpose, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
