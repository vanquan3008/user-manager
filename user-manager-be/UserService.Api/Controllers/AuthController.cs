using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Common;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Features.Auth.Queries;
using UserService.Api.Auth;
using UserService.Api.Controllers.Helpers;
using UserService.Api.Contracts.Auth;
using UserService.Api.Contracts.OAuth;
using UserService.Api.Extensions;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services;
using System.Security.Authentication;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IMediator mediator, AppSettings appSettings) : ApiControllerBase
{
    private const string RefreshTokenCookieName = AuthCookieNames.RefreshToken;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.Username,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.ClientId);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return CreatedResponse(nameof(Register), AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password,
            request.ClientId,
            request.ReturnUrl);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("sso/login")]
    [ProducesResponseType(typeof(ApiResponse<SsoLoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SsoLoginResponse>>> SsoLogin(
        [FromBody] LoginApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SsoLoginCommand(
            request.Email,
            request.Password);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(response);
    }

    [Authorize]
    [HttpPost("token-exchange")]
    [HttpPost("sso/exchange-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> ExchangeApplicationToken(
        [FromBody] AppTokenExchangeApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var updatedCommand = new ExchangeApplicationTokenCommand(
            request.TargetApplicationId,
            User.GetRequiredCurrentUserId());
        var response = await mediator.Send(updatedCommand, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [Authorize]
    [HttpPost("sso/authorize")]
    [ProducesResponseType(typeof(ApiResponse<SsoAuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SsoAuthorizeResponse>>> SsoAuthorize(
        [FromBody] SsoAuthorizeApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SsoAuthorizeCommand(
            User.GetRequiredCurrentUserId(),
            request.ApplicationId,
            request.ReturnUrl);
        
        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(response);
    }

    [Authorize]
    [HttpGet("sso/applications")]
    [ProducesResponseType(typeof(ApiResponse<GetSsoApplicationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<GetSsoApplicationsResponse>>> GetSsoApplications(
        CancellationToken cancellationToken = default)
    {
        var query = new GetSsoApplicationsQuery(User.GetRequiredCurrentUserId());
        var response = await mediator.Send(query, cancellationToken);
        return OkResponse(response);
    }

  
    [HttpPost("passwordless/request")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RequestPasswordlessLogin(
        [FromBody] RequestPasswordlessLoginApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RequestPasswordlessLoginCommand(request.Email, request.ClientId);

        await mediator.Send(command, cancellationToken);
        return OkSuccess("Send email successfully!");
    }

    [HttpGet("magic-link/verify")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyMagicLink(
        [FromQuery] string token,
        [FromQuery] string purpose,
        [FromQuery] string? clientId,
        CancellationToken cancellationToken = default)
    {
        var command = new VerifyMagicLinkCommand(token, purpose, clientId);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> VerifyEmailByToken(
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequestResponse("Token is required", "INVALID_TOKEN");
        }

        var succeeded = await mediator.Send(new VerifyEmailWithTokenCommand(token), cancellationToken);
        if (!succeeded)
        {
            return BadRequestResponse("Invalid or expired verification link", "INVALID_TOKEN");
        }

        return OkSuccess("Email verified successfully");
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> VerifyEmail(
        [FromBody] VerifyEmailApiRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequestResponse("Token is required", "INVALID_TOKEN");
        }

        if (request.UserId.HasValue)
        {
            await mediator.Send(new VerifyEmailCommand(request.UserId.Value, request.Token), cancellationToken);
            return OkSuccess("Email verified successfully");
        }

        var succeeded = await mediator.Send(new VerifyEmailWithTokenCommand(request.Token), cancellationToken);
        if (!succeeded)
        {
            return BadRequestResponse("Invalid or expired verification link", "INVALID_TOKEN");
        }

        return OkSuccess("Email verified successfully");
    }

    [HttpPost("magic-link/resend")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ResendMagicLink(
        [FromBody] ResendMagicLinkApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ResendMagicLinkCommand(request.Email, request.Purpose);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("If the email exists, a magic link has been sent");
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenApiRequest? request,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = AuthControllerHelpers.ResolveRefreshToken(Request, request?.RefreshToken, RefreshTokenCookieName);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            AuthControllerHelpers.ClearRefreshTokenCookie(Request, Response, RefreshTokenCookieName);
            return UnauthorizedResponse("Refresh token is required.");
        }

        var response = await mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("password/reset-request")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RequestPasswordReset(
        [FromBody] RequestPasswordResetApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("If the email exists, a magic link has been sent");
    }

    [HttpPost("password/reset")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ResetPassword(
        [FromBody] ResetPasswordApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Password reset successfully");
    }


    [Authorize]
    [HttpPost("password/change")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ChangePassword(
        [FromBody] ChangePasswordApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var updatedCommand = new ChangePasswordCommand(
            User.GetRequiredCurrentUserId(),
            request.CurrentPassword,
            request.NewPassword);
        await mediator.Send(updatedCommand, cancellationToken);
        return OkSuccess("Password changed successfully");
    }
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> Logout(CancellationToken cancellationToken = default)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.TryGetCurrentUserId();
            if (userId.HasValue)
            {
                await mediator.Send(new LogoutCommand(userId.Value), cancellationToken);
            }
        }
        
        AuthControllerHelpers.ClearRefreshTokenCookie(Request, Response, RefreshTokenCookieName);
        return OkSuccess("Logged out successfully");
    }

    [HttpPost("magic-link/verify")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyMagicLinkPost(
        [FromBody] VerifyMagicLinkApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new VerifyMagicLinkCommand(request.Token, request.Purpose, request.ClientId);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

}
