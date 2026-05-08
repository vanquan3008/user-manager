using MediatR;
using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Features.OAuth.Commands;

public record AuthorizeOAuthCommand(
    Guid UserId,
    OAuthAuthorizeRequest Request
) : IRequest<OAuthAuthorizeResponse>;

