using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

/// <summary>
/// Repository interface cho User entity
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyDictionary<Guid, bool>> GetIsSsoAccountByProfileIdsAsync(
        IEnumerable<Guid> profileIds,
        CancellationToken cancellationToken = default);

    /// <summary>Get multiple users by their IDs</summary>
    Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
}
