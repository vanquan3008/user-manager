namespace UserService.Application.Interfaces.Services;

public interface IBackchannelLogoutService
{
    /// <summary>
    /// Sends an OpenID Connect Back-Channel Logout 1.0 logout token to every given application
    /// that has a back-channel logout URI. Failures are logged and never thrown.
    /// </summary>
    Task NotifyApplicationsAsync(Guid userId, Guid sessionId, IReadOnlyCollection<Guid> applicationIds, CancellationToken cancellationToken = default);
}
