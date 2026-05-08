using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class UserApplicationAppService : IUserApplicationAppService
{
    private readonly IUserApplicationRepository _userApplications;

    public UserApplicationAppService(IUserApplicationRepository userApplications)
    {
        _userApplications = userApplications;
    }

    public Task<UserApplication?> GetMembershipAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
        => _userApplications.GetByUserAndAppAsync(userId, applicationId, cancellationToken);

    public async Task<IReadOnlyList<UserApplication>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _userApplications.GetByUserAsync(userId, cancellationToken);
        return items.ToList();
    }
}
