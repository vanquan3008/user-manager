using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public sealed class ApplicationCacheService : IApplicationCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ILogger<ApplicationCacheService> _logger;
    private readonly TimeSpan _cacheExpiration;
    private const string CacheKeyPrefix = "ApiKey:";

    public ApplicationCacheService(
        IMemoryCache cache,
        IApplicationRepository applicationRepository,
        ILogger<ApplicationCacheService> logger)
    {
        _cache = cache;
        _applicationRepository = applicationRepository;
        _logger = logger;
        _cacheExpiration = TimeSpan.FromMinutes(10);
    }

    public async Task<AppApplication?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var cacheKey = GetCacheKey(apiKey);
        if (_cache.TryGetValue<AppApplication?>(cacheKey, out var cachedApp))
        {
            _logger.LogDebug("API key '{ApiKey}' found in cache", MaskApiKey(apiKey));
            return cachedApp;
        }
        _logger.LogDebug("API key '{ApiKey}' not in cache, fetching from database", MaskApiKey(apiKey));
        var app = await _applicationRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            Size = 1 // For cache size limiting if configured
        };

        _cache.Set(cacheKey, app, cacheOptions);
        _logger.LogDebug("API key '{ApiKey}' cached for {Minutes} minutes", MaskApiKey(apiKey), _cacheExpiration.TotalMinutes);

        return app;
    }

    private static string GetCacheKey(string apiKey) => $"{CacheKeyPrefix}{apiKey}";

    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
            return "***";

        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }
}
