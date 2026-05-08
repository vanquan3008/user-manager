using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class ApplicationOAuthScopeRepository : Repository<ApplicationOAuthScope>, IApplicationOAuthScopeRepository
{
    public ApplicationOAuthScopeRepository(UserDbContext context) : base(context) { }

    public async Task<IReadOnlyList<string>> GetAllowedScopeCodesAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var scopes = await _dbSet.AsNoTracking()
            .Where(x => x.ApplicationId == applicationId && x.IsActive)
            .Select(x => new { x.Permission.Code, x.Permission.Scope })
            .Distinct()
            .ToListAsync(cancellationToken);
        var result = new List<string>();
        foreach (var scope in scopes)
        {
            if (!string.IsNullOrWhiteSpace(scope.Scope))
            {
                result.Add(scope.Scope);
            }
            else
            {
                result.Add(scope.Code);
            }
        }

        return result.Distinct().ToList();
    }
}

