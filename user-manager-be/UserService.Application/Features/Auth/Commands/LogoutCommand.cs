using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Ends the current session (identified by the access token's <c>sid</c> or the refresh token cookie).
/// With <paramref name="AllSessions"/> every session of the user is ended (global logout).
/// </summary>
public record LogoutCommand(
    Guid? UserId,
    Guid? SessionId,
    string? RefreshToken = null,
    bool AllSessions = false
) : IRequest<Unit>;
