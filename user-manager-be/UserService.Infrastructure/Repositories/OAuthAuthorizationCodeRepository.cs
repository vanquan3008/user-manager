using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class OAuthAuthorizationCodeRepository : Repository<OAuthAuthorizationCode>, IOAuthAuthorizationCodeRepository
{
    public OAuthAuthorizationCodeRepository(UserDbContext context) : base(context) { }

    public Task<OAuthAuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = (code ?? string.Empty).Trim();
        return _dbSet.AsTracking().FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
    }
}

