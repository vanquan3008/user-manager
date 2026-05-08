using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record LogoutCommand(
    Guid UserId
) : IRequest<Unit>;
