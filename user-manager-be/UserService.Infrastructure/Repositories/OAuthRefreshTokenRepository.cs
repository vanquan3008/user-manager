using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Repositories;

public sealed class OAuthRefreshTokenRepository : Repository<OAuthRefreshToken>, IOAuthRefreshTokenRepository
{
    private readonly IClock _clock;
    
    public OAuthRefreshTokenRepository(UserDbContext context, IClock clock) : base(context) 
    { 
        _clock = clock;
    }

    public Task<OAuthRefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var normalized = (tokenHash ?? string.Empty).Trim();
        return _dbSet.AsTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == normalized && x.RevokedAtUtc == null, cancellationToken);
    }

    public async Task RevokeAllForUserClientAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet.AsTracking()
            .Where(x => x.UserId == userId && x.ApplicationId == applicationId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var t in tokens)
            t.Revoke(DateTime.UtcNow);
    }
}

