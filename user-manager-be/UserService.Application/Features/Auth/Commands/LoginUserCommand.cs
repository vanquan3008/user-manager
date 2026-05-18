using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record LoginUserCommand(
    string Email,
    string Password,
    string? ClientId = null,
    string? ReturnUrl = null
) : IRequest<AuthResponse>;
