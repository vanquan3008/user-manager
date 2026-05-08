using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IApplicationOAuthScopeRepository : IRepository<ApplicationOAuthScope>
{
    Task<IReadOnlyList<string>> GetAllowedScopeCodesAsync(Guid applicationId, CancellationToken cancellationToken = default);
}

