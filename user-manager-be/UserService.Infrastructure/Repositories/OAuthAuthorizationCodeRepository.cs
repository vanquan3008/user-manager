using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class OAuthAuthorizationCodeRepository : Repository<OAuthAuthorizationCode>, IOAuthAuthorizationCodeRepository
{
    public OAuthAuthorizationCodeRepository(UserDbContext context) : base(context) { }

    public Task<OAuthAuthorizationCode?> GetByCodeHashAsync(string codeHash, CancellationToken cancellationToken = default)
    {
        var normalized = (codeHash ?? string.Empty).Trim();
        return _dbSet.AsTracking().FirstOrDefaultAsync(x => x.CodeHash == normalized, cancellationToken);
    }

    // Authorization codes are ephemeral: hard-delete (bypassing soft-delete) once well past expiry.
    public Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default)
        => _dbSet.IgnoreQueryFilters().Where(x => x.ExpiresAtUtc < olderThan).ExecuteDeleteAsync(cancellationToken);
}
