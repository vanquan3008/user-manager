using MediatR;

namespace UserService.Application.Features.Auth.Commands;

/// <summary>Admin thu hồi quyền truy cập app của SSO user.</summary>
public record RevokeSsoAppAccessCommand(
    Guid SsoUserId,
    Guid ApplicationId
) : IRequest<SuccessResponse>;
