namespace UserService.Application.Interfaces.Services.Security;

/// <summary>Ambient information about the current request (empty outside HTTP requests).</summary>
public interface IRequestContext
{
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    Guid? SessionId { get; }
}
