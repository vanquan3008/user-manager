using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

/// <summary>
/// Repository for UserApplication (user's membership/role in an app).
/// </summary>
public sealed class UserApplicationRepository : Repository<UserApplication>, IUserApplicationRepository
{
    public UserApplicationRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<UserApplication?> GetByUserAndAppAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.ApplicationId == applicationId, cancellationToken);
    }

    public async Task<IEnumerable<UserApplication>> GetByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(ua => ua.ApplicationId == applicationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
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

