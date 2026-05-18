using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;
public interface IApplicationCacheService
{
    Task<AppApplication?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
}
