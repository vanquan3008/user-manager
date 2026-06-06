using MediatR;

namespace UserService.Application.Features.Auth.Commands;

public record GrantSsoAppAccessCommand(
    Guid SsoUserId,
    Guid ApplicationId,
    string Role = "Admin"
) : IRequest<SuccessResponse>;
