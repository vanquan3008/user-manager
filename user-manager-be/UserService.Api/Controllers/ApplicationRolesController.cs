using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Features.ApplicationRoles.Commands;
using UserService.Application.Features.ApplicationRoles.Queries;
using UserService.Api.Authorization;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/applications/{applicationId:guid}/roles")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
public class ApplicationRolesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ApplicationRoleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationRoleDto>>>> GetRoles(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationRolesQuery(applicationId);
        var rolesList = await mediator.Send(query, cancellationToken);
        return OkResponse(rolesList);
    }

    /// <summary>
    /// Create a new role for a specific application.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> CreateRole(
        Guid applicationId,
        [FromBody] CreateApplicationRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateApplicationRoleCommand(applicationId, request.Name, request.Description);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return BadRequestResponse("Failed to create role", "ROLE_CREATE_FAILED", errors);
        }

        var roleName = request.Name.Trim();
        return OkSuccess($"Role '{roleName}' created for application");
    }

    /// <summary>
    /// Update description of a role for an application.
    /// </summary>
    [HttpPut("{roleName}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> UpdateRole(
        Guid applicationId,
        string roleName,
        [FromBody] UpdateApplicationRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateApplicationRoleCommand(applicationId, roleName, request.Description);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return result.Errors?.ContainsKey("ROLE_NOT_FOUND") == true
                ? NotFoundResponse("Role not found for this application", "ROLE_UPDATE_FAILED", errors)
                : StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, "Failed to update role", new ErrorDetail("ROLE_UPDATE_FAILED", "Failed to update role", errors)));
        }

        return OkSuccess("Role updated successfully");
    }
    [HttpDelete("{roleName}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> DeleteRole(
        Guid applicationId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteApplicationRoleCommand(applicationId, roleName);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return result.Errors?.ContainsKey("ROLE_NOT_FOUND") == true
                ? NotFoundResponse("Role not found for this application", "ROLE_DELETE_FAILED", errors)
                : StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, "Failed to delete role", new ErrorDetail("ROLE_DELETE_FAILED", "Failed to delete role", errors)));
        }
        return OkSuccess("Role deleted successfully");
    }
}
