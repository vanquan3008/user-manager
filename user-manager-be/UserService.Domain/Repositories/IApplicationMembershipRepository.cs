using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IApplicationMembershipRepository : IRepository<ApplicationMembership>
{
    Task<ApplicationMembership?> GetByUserAndAppAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationMembership>> GetByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationMembership>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
