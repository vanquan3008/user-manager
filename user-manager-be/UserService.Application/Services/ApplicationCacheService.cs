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
    private const string CacheIdPrefix = "AppId:";

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

    public async Task<AppApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        if (applicationId == Guid.Empty)
            return null;

        var cacheKey = GetCacheKeyById(applicationId);
        if (_cache.TryGetValue<AppApplication?>(cacheKey, out var cachedApp))
        {
            _logger.LogDebug("Application ID '{AppId}' found in cache", applicationId);
            return cachedApp;
        }
        _logger.LogDebug("Application ID '{AppId}' not in cache, fetching from database", applicationId);
        var app = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration,
            Size = 1
        };

        _cache.Set(cacheKey, app, cacheOptions);
        _logger.LogDebug("Application ID '{AppId}' cached for {Minutes} minutes", applicationId, _cacheExpiration.TotalMinutes);

        return app;
    }

    public void InvalidateApplication(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
            return;

        var cacheKey = GetCacheKeyById(applicationId);
        _cache.Remove(cacheKey);
        _logger.LogInformation("Invalidated cache for Application ID '{AppId}'", applicationId);
    }

    public void InvalidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return;

        var cacheKey = GetCacheKey(apiKey);
        _cache.Remove(cacheKey);
        _logger.LogInformation("Invalidated cache for API key '{ApiKey}'", MaskApiKey(apiKey));
    }

    public void ClearAll()
    { 
        _logger.LogWarning("ClearAll called but IMemoryCache doesn't support bulk clear. Consider using distributed cache.");
    }

    private static string GetCacheKey(string apiKey) => $"{CacheKeyPrefix}{apiKey}";
    private static string GetCacheKeyById(Guid id) => $"{CacheIdPrefix}{id}";

    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
            return "***";

        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }
}
