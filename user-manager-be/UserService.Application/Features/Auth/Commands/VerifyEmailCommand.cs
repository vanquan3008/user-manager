using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Verify email with magic link token
/// </summary>
public record VerifyEmailCommand(
    Guid UserId,
    string Token
) : IRequest<Unit>;
