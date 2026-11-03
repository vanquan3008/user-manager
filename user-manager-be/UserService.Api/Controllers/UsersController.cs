using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Api.Contracts.Users;
using UserService.Api.Extensions;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Features.Users.Queries;
using UserService.Domain.Constants;
using UserService.Api.Contracts.Account;
using UserService.Application.Interfaces.Services.Security;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewUsers)]
    [ProducesResponseType(typeof(PagedResponse<IdentityUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<IdentityUserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(page, 1);
        bool? sso = type?.ToLowerInvariant() switch { "sso" => true, "console" => false, _ => null };
        var normalizedPageSize = pageSize is < 1 or > 100 ? 50 : pageSize;

        var response = await mediator.Send(
            new GetUsersQuery(normalizedPage, normalizedPageSize, search, sso),
            cancellationToken);

        return Ok(response);
    }
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserWithRolesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserWithRolesDto>>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var user = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);

        if (user == null)
        {
            return NotFoundResponse("User not found");
        }

        return OkResponse(user);
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ViewUsers)]
    [ProducesResponseType(typeof(ApiResponse<UserWithRolesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserWithRolesDto>>> GetUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);

        if (user == null)
        {
            return NotFoundResponse("User not found");
        }

        return OkResponse(user);
    }
    [HttpPost("{userId:guid}/roles/upgrade")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> UpgradeUserRole(
        Guid userId,
        [FromBody] UpgradeUserRoleApiRequest request,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new AddToRoleCommand(userId, request.Role), cancellationToken);

        return OkSuccess($"Role '{request.Role}' has been granted to user {userId}.");
    }
    [HttpDelete("{userId:guid}/roles/{roleName}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RemoveUserRole(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RemoveFromRoleCommand(userId, roleName), cancellationToken);

        return OkSuccess($"Role '{roleName}' has been removed from user {userId}.");
    }

    [HttpPost("sso")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> CreateSSOUser(
        [FromBody] CreateSSOUserApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSsoUserCommand(
            request.Email,
            request.Password,
            request.Username,
            request.FirstName,
            request.LastName,
            request.DateOfBirth);
        var result = await mediator.Send(command, cancellationToken);
        return CreatedResponse(nameof(CreateSSOUser), result);
    }

    [HttpPost("sso/{ssoUserId:guid}/applications")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> GrantSSOAppAccess(
        Guid ssoUserId,
        [FromBody] GrantSSOAppAccessApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GrantSsoAppAccessCommand(ssoUserId, request.ApplicationId, request.Role ?? RoleNames.Admin),
            cancellationToken);

        return OkResponse(result);
    }
    [HttpDelete("sso/{ssoUserId:guid}/applications/{applicationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RevokeSSOAppAccess(
        Guid ssoUserId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new RevokeSsoAppAccessCommand(ssoUserId, applicationId),
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("sso/{ssoUserId:guid}/applications")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SSOUserApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SSOUserApplicationDto>>>> GetSSOUserApplications(
        Guid ssoUserId,
        CancellationToken cancellationToken = default)
    {
        var applications = await mediator.Send(new GetSSOUserApplicationsQuery(ssoUserId), cancellationToken);
        return OkResponse(applications);
    }

    [HttpPost("{userId:guid}/ban")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> BanUser(
        Guid userId,
        [FromBody] BanUserApiRequest? body,
        CancellationToken cancellationToken = default)
    {
        var actorId = User.GetRequiredCurrentUserId();
        var result = await mediator.Send(new BanUserCommand(userId, actorId, body?.Reason), cancellationToken);
        return OkResponse(result);
    }
    [HttpPost("{userId:guid}/unban")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> UnbanUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var actorId = User.GetRequiredCurrentUserId();
        var result = await mediator.Send(new UnbanUserCommand(userId, actorId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>Active sessions (devices) of a user.</summary>
    [HttpGet("{userId:guid}/sessions")]
    [Authorize(Policy = AuthorizationPolicies.ViewUsers)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SessionDto>>>> GetUserSessions(
        Guid userId,
        [FromServices] ISessionService sessions,
        CancellationToken cancellationToken = default)
    {
        var list = await sessions.ListActiveAsync(userId, cancellationToken);
        return OkResponse<IReadOnlyList<SessionDto>>(list.Select(x => AccountController.ToDto(x, User.TryGetSessionId())).ToList());
    }

    /// <summary>Signs the user out everywhere: revokes all sessions and OAuth grants (with back-channel logout).</summary>
    [HttpPost("{userId:guid}/sessions/revoke")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RevokeUserSessions(
        Guid userId,
        [FromServices] ISessionService sessions,
        [FromServices] IOAuthRefreshTokenRepository oauthTokens,
        [FromServices] IUnitOfWork unitOfWork,
        [FromServices] IAuditLogger audit,
        CancellationToken cancellationToken = default)
    {
        var count = await sessions.RevokeAllAsync(userId, SessionRevocationReasons.AdminRevoked, cancellationToken: cancellationToken);
        await oauthTokens.RevokeAllForUserAsync(userId, SessionRevocationReasons.AdminRevoked, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditEventTypes.SessionsRevokedAll, true, new AuditEntry(
            TargetUserId: userId, TargetType: "user", TargetId: userId.ToString(), Details: new { sessions = count }), cancellationToken);
        return OkSuccess($"Revoked {count} session(s) and all application grants.");
    }

    [HttpGet("{userId:guid}/mfa")]
    [Authorize(Policy = AuthorizationPolicies.ViewUsers)]
    public async Task<ActionResult<ApiResponse<MfaStatus>>> GetUserMfa(
        Guid userId,
        [FromServices] IMfaService mfa,
        CancellationToken cancellationToken = default)
        => OkResponse(await mfa.GetStatusAsync(userId, cancellationToken));

    /// <summary>Removes a user's authenticator so they can re-enrol (e.g. lost device). Requires SECURITY.MANAGE.</summary>
    [HttpPost("{userId:guid}/mfa/reset")]
    [Authorize(Policy = AuthorizationPolicies.ManageSecurity)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ResetUserMfa(
        Guid userId,
        [FromServices] IMfaService mfa,
        [FromServices] ISessionService sessions,
        [FromServices] IAuditLogger audit,
        CancellationToken cancellationToken = default)
    {
        await mfa.AdminResetAsync(userId, cancellationToken);
        await sessions.RevokeAllAsync(userId, SessionRevocationReasons.MfaChanged, cancellationToken: cancellationToken);
        await audit.LogAsync(AuditEventTypes.MfaDisabled, true, new AuditEntry(
            TargetUserId: userId, TargetType: "user", TargetId: userId.ToString(), Reason: "admin_reset"), cancellationToken);
        return OkSuccess("MFA has been reset. The user must enrol again.");
    }
}
