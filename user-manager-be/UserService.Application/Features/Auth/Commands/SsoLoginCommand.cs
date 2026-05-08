using MediatR;
namespace UserService.Application.Features.Auth.Commands;

public record SsoLoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;

