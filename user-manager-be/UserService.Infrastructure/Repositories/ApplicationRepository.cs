using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class ApplicationRepository : Repository<AppApplication>, IApplicationRepository
{
    public ApplicationRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AppApplication>> GetAllActiveWithOwnerAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(a => a.Owner)
            .Where(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }
    public async Task<AppApplication?> GetByIdWithOwnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
    public async Task<AppApplication?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ApiKey == apiKey, cancellationToken);
    }
    public async Task<IReadOnlyList<AppApplication>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<AppApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(a => a.Users.Any(u => u.UserId == userId && u.IsActive))
            .ToListAsync(cancellationToken);
    }
    public async Task<bool> IsNameTakenAsync(Guid ownerId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(a => a.OwnerId == ownerId && a.Name == name, cancellationToken);
    }
}

