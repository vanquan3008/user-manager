using System.Security.Claims;
using UserService.Api.Middleware;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Constants;

namespace UserService.Api.Security;

public sealed class HttpRequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpRequestContext(IHttpContextAccessor accessor) => _accessor = accessor;

    private HttpContext? Http => _accessor.HttpContext;

    public string? IpAddress => Http?.GetRealIpAddress();
    public string? UserAgent => Http?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
    public string? CorrelationId => Http?.Items["CorrelationId"]?.ToString() ?? Http?.TraceIdentifier;

    public Guid? UserId => Guid.TryParse(
        Http?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Http?.User.FindFirstValue("sub"), out var id) ? id : null;

    public string? UserEmail => Http?.User.FindFirstValue(ClaimTypes.Email) ?? Http?.User.FindFirstValue("email");

    public Guid? SessionId => Guid.TryParse(Http?.User.FindFirstValue(JwtClaimNames.SessionId), out var sid) ? sid : null;
}
