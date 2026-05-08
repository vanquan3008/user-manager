using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UserOAuthConsentRepository : Repository<UserOAuthConsent>, IUserOAuthConsentRepository
{
    public UserOAuthConsentRepository(UserDbContext context) : base(context) { }

    public async Task<UserOAuthConsent?> GetActiveConsentAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Scopes)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ApplicationId == applicationId && x.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetGrantedScopesAsync(Guid consentId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthConsentScope>()
            .AsNoTracking()
            .Where(x => x.ConsentId == consentId)
            .Select(x => x.Permission.Scope ?? x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

