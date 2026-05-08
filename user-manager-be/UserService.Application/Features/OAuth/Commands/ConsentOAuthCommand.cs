using MediatR;
using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Features.OAuth.Commands;

public record ConsentOAuthCommand(
    Guid UserId,
    OAuthConsentRequest Request
) : IRequest<OAuthAuthorizeResponse>;

