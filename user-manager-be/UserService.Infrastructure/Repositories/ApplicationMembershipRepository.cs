using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class ApplicationMembershipRepository : Repository<ApplicationMembership>, IApplicationMembershipRepository
{
    public ApplicationMembershipRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<ApplicationMembership?> GetByUserAndAppAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.ApplicationId == applicationId, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationMembership>> GetByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(ua => ua.User)
            .Where(ua => ua.ApplicationId == applicationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationMembership>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(ua => ua.Application)
            .Where(ua => ua.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .CountAsync(ua => ua.ApplicationId == applicationId, cancellationToken);
    }
}

