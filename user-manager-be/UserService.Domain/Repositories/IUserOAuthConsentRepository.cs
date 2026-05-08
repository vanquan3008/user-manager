using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IUserOAuthConsentRepository : IRepository<UserOAuthConsent>
{
    Task<UserOAuthConsent?> GetActiveConsentAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetGrantedScopesAsync(Guid consentId, CancellationToken cancellationToken = default);
}

