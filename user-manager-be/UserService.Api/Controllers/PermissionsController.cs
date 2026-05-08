using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserService.Api.Authorization;
using UserService.Application.Features.Permissions.Commands;
using UserService.Application.Features.Permissions.Queries;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PermissionsController(IMediator mediator) : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CreatePermission)]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> CreatePermission(
        [FromBody] CreatePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePermissionCommand(
            request.Code,
            request.Description,
            request.Category,
            request.Scope);

        var response = await mediator.Send(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetPermissionById), new { permissionId = response.Data?.Id }, response);
    }
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewPermissions)]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAllPermissions(
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetAllPermissionsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{permissionId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ViewPermissions)]
    [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetPermissionById(
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetPermissionByIdQuery(permissionId), cancellationToken);
        return Ok(response);
    }

    [HttpPost("{roleId:guid}/permissions")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> AssignPermissionToRole(
        Guid roleId,
        [FromBody] AssignPermissionToRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AssignPermissionToRoleCommand(roleId, request.PermissionId);
        var response = await mediator.Send(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{roleId:guid}/permissions/batch")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> AssignPermissionsToRole(
        Guid roleId,
        [FromBody] AssignPermissionsToRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AssignPermissionsToRoleCommand(roleId, request.PermissionIds);
        var response = await mediator.Send(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RemovePermissionFromRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemovePermissionFromRoleCommand(roleId, permissionId);
        var response = await mediator.Send(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
    [HttpGet("{roleId:guid}/permissions")]
    [Authorize(Policy = AuthorizationPolicies.ViewRoles)]
    [ProducesResponseType(typeof(ApiResponse<RoleWithPermissionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleWithPermissionsDto>>> GetRolePermissions(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetRolePermissionsQuery(roleId), cancellationToken);
        return Ok(response);
    }
}
