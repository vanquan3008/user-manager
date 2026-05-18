using MediatR;
namespace UserService.Application.Features.Auth.Commands;

public record SSOLoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;

