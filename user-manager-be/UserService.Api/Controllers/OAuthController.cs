using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Controllers.Helpers;
using UserService.Api.Contracts.OAuth;
using UserService.Application.DTOs.OAuth;
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
        [FromQuery] OAuthAuthorizeQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var request = OAuthQueryRequestBinding.ToAuthorizeRequest(query, Request.Query);

        var result = await _mediator.Send(new AuthorizeOAuthCommand(userId, request), cancellationToken);
        
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
        [FromBody] OAuthConsentApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var consentRequest = new OAuthConsentRequest(
            request.ClientId,
            request.RedirectUri,
            request.Scopes,
            request.State,
            request.CodeChallenge,
            request.CodeChallengeMethod,
            request.Approve);
        var result = await _mediator.Send(new ConsentOAuthCommand(userId, consentRequest), cancellationToken);
        return OkResponse(result);
    }
    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<OAuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OAuthTokenResponse>>> Token(
        [FromBody] OAuthTokenApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenRequest = new OAuthTokenRequest(
            request.GrantType,
            request.ClientId,
            request.RedirectUri,
            request.ClientSecret,
            request.Code,
            request.CodeVerifier,
            request.RefreshToken);
        var result = await _mediator.Send(new ExchangeOAuthCodeCommand(tokenRequest), cancellationToken);
        return OkResponse(result);
    }
}

