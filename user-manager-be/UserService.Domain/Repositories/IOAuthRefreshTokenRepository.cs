using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IOAuthRefreshTokenRepository : IRepository<OAuthRefreshToken>
{
    Task<OAuthRefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllForUserClientAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
}

