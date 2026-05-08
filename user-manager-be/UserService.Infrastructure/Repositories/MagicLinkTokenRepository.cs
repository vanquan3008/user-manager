using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Magic link token repository - extends base Repository, uses IMagicLinkTokenRepository.
/// </summary>
public class MagicLinkTokenRepository : Repository<MagicLinkToken>, IMagicLinkTokenRepository
{
    public MagicLinkTokenRepository(UserDbContext context) : base(context)
    {
    }
    public async Task<MagicLinkToken?> GetValidTokenAsync(string token, string purpose, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => 
                t.Token == token && 
                t.Purpose == purpose && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow, 
                cancellationToken);
    }

    public async Task InvalidateAllTokensAsync(Guid userId, string purpose, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(t => t.UserId == userId && t.Purpose == purpose && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.MarkAsUsed(DateTime.UtcNow);
        }
        await Task.CompletedTask;
    }

    public async Task<MagicLinkToken?> GetLatestTokenAsync(Guid userId, string purpose, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Purpose == purpose)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _dbSet
            .Where(t => (t.ExpiresAt < DateTime.UtcNow || t.IsUsed) && t.CreatedAt < DateTime.UtcNow.AddDays(-7))
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(expiredTokens);
        return expiredTokens.Count;
    }
}
