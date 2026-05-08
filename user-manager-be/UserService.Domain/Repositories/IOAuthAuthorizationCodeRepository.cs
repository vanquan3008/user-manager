using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IOAuthAuthorizationCodeRepository : IRepository<OAuthAuthorizationCode>
{
    Task<OAuthAuthorizationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

