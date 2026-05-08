using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record LoginUserCommand(
    string Email,
    string Password,
    Guid? ApplicationId = null,
    string? ReturnUrl = null
) : IRequest<AuthResponse>;
