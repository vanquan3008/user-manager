using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;
public interface IApplicationCacheService
{
    Task<AppApplication?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<AppApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);

    void InvalidateApplication(Guid applicationId);
    void InvalidateApiKey(string apiKey);
    void ClearAll();
}
