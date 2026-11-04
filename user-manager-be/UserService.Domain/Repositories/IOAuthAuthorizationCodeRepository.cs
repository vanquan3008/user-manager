using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IOAuthAuthorizationCodeRepository : IRepository<OAuthAuthorizationCode>
{
    Task<OAuthAuthorizationCode?> GetByCodeHashAsync(string codeHash, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
