using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;

public interface IUserApplicationAppService
{
    Task<UserApplication?> GetMembershipAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
