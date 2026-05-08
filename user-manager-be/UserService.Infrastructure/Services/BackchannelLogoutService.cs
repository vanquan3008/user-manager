using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

public class BackchannelLogoutService : IBackchannelLogoutService
{
    private readonly ITokenService _tokenService;
    private readonly IThirdPartyApiService _thirdPartyApiService;
    private readonly ILogger<BackchannelLogoutService> _logger;

    public BackchannelLogoutService(
        ITokenService tokenService,
        IThirdPartyApiService thirdPartyApiService,
        ILogger<BackchannelLogoutService> logger)
    {
        _tokenService = tokenService;
        _thirdPartyApiService = thirdPartyApiService;
        _logger = logger;
    }

    public async Task SendLogoutTokenAsync(Guid userId, string sessionId, string backchannelLogoutUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backchannelLogoutUri))
            return;

        try
        {
            var logoutToken = _tokenService.GenerateLogoutToken(userId, sessionId);

            var response = await _thirdPartyApiService.PostFormAsync(
                "backchannel_logout",
                backchannelLogoutUri,
                new Dictionary<string, string>
                {
                    ["logout_token"] = logoutToken
                },
                cancellationToken);

            if (!response.Success)
            {
                _logger.LogWarning(
                    "Backchannel logout to {Uri} failed with status {StatusCode}. Body: {Body}",
                    backchannelLogoutUri,
                    response.StatusCode,
                    response.RawResponse);
                return;
            }

            _logger.LogInformation("Successfully sent backchannel logout to {Uri}", backchannelLogoutUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending backchannel logout to {Uri}", backchannelLogoutUri);
        }
    }
}
