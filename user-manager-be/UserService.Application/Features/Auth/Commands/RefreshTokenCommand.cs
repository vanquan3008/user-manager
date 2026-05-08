using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Làm mới access token bằng refresh token.</summary>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;
