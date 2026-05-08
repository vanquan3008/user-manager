using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Verify magic link token for passwordless login
/// </summary>
public record VerifyMagicLinkCommand(
    string Token,
    string Purpose,
    Guid? ApplicationId = null
) : IRequest<AuthResponse>;
