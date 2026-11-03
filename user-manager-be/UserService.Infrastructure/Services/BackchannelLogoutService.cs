using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Services;

/// <summary>OpenID Connect Back-Channel Logout 1.0 sender.</summary>
public class BackchannelLogoutService : IBackchannelLogoutService
{
    private static readonly TimeSpan PerRequestTimeout = TimeSpan.FromSeconds(5);

    private readonly ITokenService _tokenService;
    private readonly IThirdPartyApiService _thirdPartyApiService;
    private readonly IApplicationRepository _applications;
    private readonly ILogger<BackchannelLogoutService> _logger;

    public BackchannelLogoutService(
        ITokenService tokenService,
        IThirdPartyApiService thirdPartyApiService,
        IApplicationRepository applications,
        ILogger<BackchannelLogoutService> logger)
    {
        _tokenService = tokenService;
        _thirdPartyApiService = thirdPartyApiService;
        _applications = applications;
        _logger = logger;
    }

    public async Task NotifyApplicationsAsync(Guid userId, Guid sessionId, IReadOnlyCollection<Guid> applicationIds, CancellationToken cancellationToken = default)
    {
        var targets = new List<(string ClientId, string Uri)>();
        foreach (var applicationId in applicationIds.Distinct())
        {
            var app = await _applications.GetByIdAsync(applicationId, cancellationToken);
            var uri = app?.BackchannelLogoutUri?.Value;
            if (app is null || !app.IsActive || string.IsNullOrWhiteSpace(uri))
                continue;
            targets.Add((app.ApiKey.Value, uri));
        }

        // Notify relying parties in parallel with a short timeout so logout latency stays bounded.
        await Task.WhenAll(targets.Select(t => SendAsync(userId, sessionId, t.ClientId, t.Uri)));
    }

    private async Task SendAsync(Guid userId, Guid sessionId, string clientId, string uri)
    {
        try
        {
            using var cts = new CancellationTokenSource(PerRequestTimeout);
            var logoutToken = _tokenService.GenerateLogoutToken(userId, sessionId.ToString(), clientId);
            var response = await _thirdPartyApiService.PostFormAsync(
                "backchannel_logout",
                uri,
                new Dictionary<string, string> { ["logout_token"] = logoutToken },
                cts.Token);

            if (!response.Success)
                _logger.LogWarning("Back-channel logout to {Uri} failed with status {StatusCode}", uri, response.StatusCode);
            else
                _logger.LogInformation("Back-channel logout delivered to {Uri}", uri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Back-channel logout to {Uri} failed", uri);
        }
    }
}
