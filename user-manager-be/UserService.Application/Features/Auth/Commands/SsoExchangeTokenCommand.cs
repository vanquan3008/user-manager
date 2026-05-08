using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Exchange an existing SSO token for an app-specific token.
/// Allows: login to App A → automatically access App B without re-entering credentials.
/// </summary>
public record SsoExchangeTokenCommand(
    Guid TargetApplicationId,
    Guid UserId = default
) : IRequest<AuthResponse>;
