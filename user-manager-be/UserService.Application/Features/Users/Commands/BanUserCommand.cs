using MediatR;
using UserService.Application.DTOs.Auth;

namespace UserService.Application.Features.Users.Commands;

public record BanUserCommand(Guid UserId, Guid ActorUserId, string? Reason) : IRequest<SuccessResponse>;
