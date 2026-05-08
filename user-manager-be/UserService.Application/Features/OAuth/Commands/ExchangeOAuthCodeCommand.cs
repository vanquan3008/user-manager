using MediatR;
using UserService.Application.DTOs.OAuth;

namespace UserService.Application.Features.OAuth.Commands;

public record ExchangeOAuthCodeCommand(
    OAuthTokenRequest Request
) : IRequest<OAuthTokenResponse>;

