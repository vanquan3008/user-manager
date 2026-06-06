using MediatR;
using UserService.Application.DTOs.Auth;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>
/// Exchanges the current authenticated user context for a token scoped to another application.
/// This is an internal token-exchange flow, not a browser login flow.
/// </summary>
public record ExchangeApplicationTokenCommand(
    Guid TargetApplicationId,
    Guid UserId = default
) : IRequest<AuthResponse>;
