using MediatR;
using UserService.Application.Features.Auth.Commands;

namespace UserService.Application.Features.Auth.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationMembershipService _userApplicationService;
    private readonly IApplicationAppService _applicationService;
    private readonly IBackchannelLogoutService _backchannelLogoutService;

    public LogoutCommandHandler(
        IIdentityService identityService,
        IApplicationMembershipService userApplicationService,
        IApplicationAppService applicationService,
        IBackchannelLogoutService backchannelLogoutService)
    {
        _identityService = identityService;
        _userApplicationService = userApplicationService;
        _applicationService = applicationService;
        _backchannelLogoutService = backchannelLogoutService;
    }

    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _identityService.UpdateRefreshTokenAsync(
            command.UserId,
            string.Empty,
            DateTime.UtcNow,
            cancellationToken);

        var userApps = await _userApplicationService.GetByUserAsync(command.UserId, cancellationToken);
        var sessionId = Guid.NewGuid().ToString();

        foreach (var ua in userApps)
        {
            if (!ua.IsActive) continue;
            
            var app = await _applicationService.GetByIdAsync(ua.ApplicationId, cancellationToken);
            if (app != null && app.IsActive && app.BackchannelLogoutUri is not null && !string.IsNullOrWhiteSpace(app.BackchannelLogoutUri.Value))
            {
                await _backchannelLogoutService.SendLogoutTokenAsync(command.UserId, sessionId, app.BackchannelLogoutUri.Value, cancellationToken);
            }
        }

        return Unit.Value;
    }
}
