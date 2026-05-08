using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.Application.Configuration;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Auth;
using UserService.Application.Features.Auth.Commands;
using UserService.Api.Auth;
using UserService.Api.Controllers.Helpers;
using UserService.Api.Extensions;
using UserService.Application.Common;
using UserService.Application.Interfaces.Services;
using System.Security.Authentication;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IMediator mediator, AppSettings appSettings, IApplicationCacheService applicationCacheService) : ApiControllerBase
{
    private const string RefreshTokenCookieName = AuthCookieNames.RefreshToken;

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var applicationId = await ResolveApplicationIdAsync(request.ClientId, cancellationToken);
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.Username,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            applicationId);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return CreatedResponse(nameof(Register), AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var applicationId = await ResolveApplicationIdAsync(request.ClientId, cancellationToken);
        var command = new LoginUserCommand(
            request.Email,
            request.Password,
            applicationId,
            request.ReturnUrl);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("sso/login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SsoLogin(
        [FromBody] SsoLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    [HttpPost("sso/authorize")]
    [ProducesResponseType(typeof(ApiResponse<SsoAuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SsoAuthorizeResponse>>> SsoAuthorize(
        [FromBody] SsoAuthorizeCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken);
        if (response.RequiresConsent)
        {
            return OkResponse(response);
        }
        var redirectUrl = OAuthHelpers.BuildRedirectUrl(response.RedirectUri, response.Code!, response.State);
        return OkResponse(response with { RedirectUri = redirectUrl });
    }

    [HttpPost("sso/consent")]
    [ProducesResponseType(typeof(ApiResponse<SsoAuthorizeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SsoAuthorizeResponse>>> SsoConsent(
        [FromBody] SsoConsentCommand command,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(command, cancellationToken);
        var redirectUrl = OAuthHelpers.BuildRedirectUrl(response.RedirectUri, response.Code!, response.State);
        return OkResponse(response with { RedirectUri = redirectUrl });
    }


    [Authorize]
    [HttpPost("sso/exchange-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SsoExchangeToken(
        [FromBody] SsoExchangeTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var updatedCommand = command with { UserId = User.GetRequiredCurrentUserId() };
        var response = await mediator.Send(updatedCommand, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

  
    [HttpPost("passwordless/request")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RequestPasswordlessLogin(
        [FromBody] RequestPasswordlessLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var applicationId = await ResolveApplicationIdAsync(request.ClientId, cancellationToken);
        var command = new RequestPasswordlessLoginCommand(request.Email, applicationId);

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
        var applicationId = await ResolveApplicationIdAsync(clientId, cancellationToken);
        var command = new VerifyMagicLinkCommand(token, purpose, applicationId);

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
        [FromBody] VerifyEmailRequest request,
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
        [FromBody] ResendMagicLinkCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return OkSuccess("If the email exists, a magic link has been sent");
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenCommand? command,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = AuthControllerHelpers.ResolveRefreshToken(Request, command, RefreshTokenCookieName);
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
        [FromBody] RequestPasswordResetCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return OkSuccess("If the email exists, a magic link has been sent");
    }

    [HttpPost("password/reset")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Password reset successfully");
    }


    [Authorize]
    [HttpPost("password/change")]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var updatedCommand = command with { UserId = User.GetRequiredCurrentUserId() };
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
        [FromBody] VerifyMagicLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var applicationId = await ResolveApplicationIdAsync(request.ClientId, cancellationToken);
        var command = new VerifyMagicLinkCommand(request.Token, request.Purpose, applicationId);

        var response = await mediator.Send(command, cancellationToken);
        AuthControllerHelpers.IssueRefreshTokenCookie(Request, Response, appSettings, RefreshTokenCookieName, response.RefreshToken);
        return OkResponse(AuthControllerHelpers.ToClientAuthResponse(response));
    }

    // Application id / api key lookup is provided by HttpContextApplicationExtensions

    private async Task<Guid?> ResolveApplicationIdAsync(string? clientId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return null;
        }

        var app = await applicationCacheService.GetByApiKeyAsync(clientId.Trim(), cancellationToken);
        if (app == null || !app.IsActive)
        {
            throw new AuthenticationException("Invalid or inactive client id.");
        }

        return app.Id;
    }
}
