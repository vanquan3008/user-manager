using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Features.Roles.Commands;
using UserService.Application.Features.Roles.Queries;
using UserService.Api.Authorization;
namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
public class RolesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetRoles(CancellationToken cancellationToken = default)
    {
        var query = new GetRolesQuery();
        var roles = await mediator.Send(query, cancellationToken);
        return OkResponse(roles);
    }

    [HttpGet("{roleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(Guid roleId, CancellationToken cancellationToken = default)
    {
        var query = new GetRoleByIdQuery(roleId);
        var role = await mediator.Send(query, cancellationToken);

        if (role == null)
        {
            return NotFoundResponse("Role not found");
        }

        return OkResponse(role);
    }
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateRoleCommand(request.Name, request.Description, request.Permissions);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return BadRequestResponse("Failed to create role", "ROLE_CREATE_FAILED", errors);
        }

        var roleName = request.Name.Trim();
        return OkSuccess($"Role '{roleName}' created successfully");
    }

    [HttpPut("{roleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> UpdateRole(
        Guid roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateRoleCommand(roleId, request.Description, request.Permissions);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return result.Errors?.ContainsKey("ROLE_NOT_FOUND") == true
                ? NotFoundResponse("Role not found", "ROLE_UPDATE_FAILED", errors)
                : StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, "Failed to update role", new ErrorDetail("ROLE_UPDATE_FAILED", "Failed to update role", errors)));
        }

        return OkSuccess("Role updated successfully");
    }

    /// <summary>
    /// Xóa role. Chỉ Admin và SuperAdmin.
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> DeleteRole(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteRoleCommand(roleId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors != null ? new Dictionary<string, string[]>(result.Errors) : null;
            return result.Errors?.ContainsKey("ROLE_NOT_FOUND") == true
                ? NotFoundResponse("Role not found", "ROLE_DELETE_FAILED", errors)
                : StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, null, "Failed to delete role", new ErrorDetail("ROLE_DELETE_FAILED", "Failed to delete role", errors)));
        }

        return OkSuccess("Role deleted successfully");
    }
}
