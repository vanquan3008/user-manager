using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record RequestPasswordlessLoginCommand(
    string Email,
    string? ClientId = null
) : IRequest<Unit>;
