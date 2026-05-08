using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Api.Extensions;
using UserService.Application.Features.Auth.Commands;
using UserService.Application.Features.Users.Commands;
using UserService.Application.Features.Users.Queries;
using UserService.Domain.Constants;

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
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = pageSize is < 1 or > 100 ? 50 : pageSize;

        var response = await mediator.Send(
            new GetUsersQuery(normalizedPage, normalizedPageSize, search),
            cancellationToken);

        return Ok(response);
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
        [FromBody] UpgradeUserRoleRequest request,
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
        [FromBody] CreateSSOUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedResponse(nameof(CreateSSOUser), result);
    }

    [HttpPost("sso/{ssoUserId:guid}/applications")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> GrantSSOAppAccess(
        Guid ssoUserId,
        [FromBody] GrantSSOAppAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GrantSSOAppAccessCommand(ssoUserId, request.ApplicationId, request.Role ?? RoleNames.Admin),
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
            new RevokeSSOAppAccessCommand(ssoUserId, applicationId),
            cancellationToken);

        return OkResponse(result);
    }

    [HttpGet("sso/{ssoUserId:guid}/applications")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SsoUserApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SsoUserApplicationDto>>>> GetSsoUserApplications(
        Guid ssoUserId,
        CancellationToken cancellationToken = default)
    {
        var applications = await mediator.Send(new GetSsoUserApplicationsQuery(ssoUserId), cancellationToken);
        return OkResponse(applications);
    }

    [HttpPost("{userId:guid}/ban")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> BanUser(
        Guid userId,
        [FromBody] BanUserRequest? body,
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
}
