using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IApplicationRepository : IRepository<AppApplication>
{
    Task<IReadOnlyList<AppApplication>> GetAllActiveWithOwnerAsync(CancellationToken cancellationToken = default);
    Task<AppApplication?> GetByIdWithOwnerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppApplication?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppApplication>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsNameTakenAsync(Guid ownerId, string name, CancellationToken cancellationToken = default);
}
