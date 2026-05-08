using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IUserApplicationRepository : IRepository<UserApplication>
{
    Task<UserApplication?> GetByUserAndAppAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserApplication>> GetByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
