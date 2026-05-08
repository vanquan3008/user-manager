using System;
using System.Threading;
using System.Threading.Tasks;

namespace UserService.Application.Interfaces.Services;

public interface IBackchannelLogoutService
{
    Task SendLogoutTokenAsync(Guid userId, string sessionId, string backchannelLogoutUri, CancellationToken cancellationToken = default);
}
