using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;

public interface IApplicationMembershipService
{
    Task<ApplicationMembership?> GetMembershipAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationMembership>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddUserAsync(Guid applicationId, Guid userId, string? role, CancellationToken cancellationToken = default);
    Task RemoveUserAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
}
