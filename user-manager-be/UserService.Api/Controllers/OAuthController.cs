using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Features.OAuth.Commands;
using UserService.Api.Extensions;
using UserService.Application.Common;


namespace UserService.Api.Controllers;

[ApiController]
[Route("api/oauth")]
[Produces("application/json")]
public class OAuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize]
    [HttpGet("authorize")]
    [ProducesResponseType(typeof(ApiResponse<OAuthAuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OAuthAuthorizeResponse>>> Authorize(
        [FromQuery] OAuthAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var scopes = request.Scopes;
        if (scopes == null || scopes.Length == 0)
        {
            if (!string.IsNullOrWhiteSpace(request.Scope))
            {
                scopes = request.Scope.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        var result = await _mediator.Send(new AuthorizeOAuthCommand(userId, request with { Scopes = scopes }), cancellationToken);
        
        if (!string.IsNullOrEmpty(result.Code))
        {
            var fullRedirectUrl = OAuthHelpers.BuildRedirectUrl(result.RedirectUri, result.Code, result.State);
            return OkResponse(result with { RedirectUri = fullRedirectUrl });
        }

        return OkResponse(result);
    }



    [Authorize]
    [HttpPost("consent")]
    [ProducesResponseType(typeof(ApiResponse<OAuthAuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OAuthAuthorizeResponse>>> Consent(
        [FromBody] OAuthConsentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var result = await _mediator.Send(new ConsentOAuthCommand(userId, request), cancellationToken);
        return OkResponse(result);
    }
    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<OAuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OAuthTokenResponse>>> Token(
        [FromBody] OAuthTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ExchangeOAuthCodeCommand(request), cancellationToken);
        return OkResponse(result);
    }
}

