using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
      
    }

    public async Task<IReadOnlyDictionary<Guid, bool>> GetIsSsoAccountByProfileIdsAsync(
        IEnumerable<Guid> profileIds,
        CancellationToken cancellationToken = default)
    {
        var ids = profileIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, bool>();

        var rows = await _dbSet.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.IsSSOAccount })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Id, x => x.IsSSOAccount);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<User>();

        return await _dbSet.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.Value == username, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }
    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(u => u.Username.Value == username, cancellationToken);
    }
    public async Task<IReadOnlyList<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u =>
                EF.Functions.Like(u.Username.Value, $"%{searchTerm}%") ||
                EF.Functions.Like(u.Email.Value, $"%{searchTerm}%") ||
                EF.Functions.Like(u.Name.DisplayName, $"%{searchTerm}%"))
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
