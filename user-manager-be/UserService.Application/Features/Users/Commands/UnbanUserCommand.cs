using MediatR;
using UserService.Application.DTOs.Auth;

namespace UserService.Application.Features.Users.Commands;

public record UnbanUserCommand(Guid UserId, Guid ActorUserId) : IRequest<SuccessResponse>;
