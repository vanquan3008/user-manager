using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record RequestPasswordlessLoginCommand(
    string Email,
    Guid? ApplicationId = null
) : IRequest<Unit>;
