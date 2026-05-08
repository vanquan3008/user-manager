using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Api.Extensions;
using UserService.Application.DTOs.Applications;
using UserService.Application.DTOs.Common;
using UserService.Application.Features.Applications.Commands;
using UserService.Application.Features.Applications.Queries;
using UserService.Domain.Constants;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ApplicationsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewApplications)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CreateApplicationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CreateApplicationResponse>>>> GetApplications(
        CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.SuperAdmin);
        var userId = isAdmin ? (Guid?)null : User.GetRequiredCurrentUserId();

        var apps = await mediator.Send(new GetApplicationsQuery(userId, isAdmin), cancellationToken);
        return OkResponse(apps);
    }

    [HttpGet("{applicationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ViewApplications)]
    [ProducesResponseType(typeof(ApiResponse<CreateApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CreateApplicationResponse>>> GetApplication(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await mediator.Send(new GetApplicationByIdQuery(applicationId), cancellationToken);
        return OkResponse(application);
    }

    [HttpDelete("{applicationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> DeleteApplication(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteApplicationCommand(applicationId), cancellationToken);
        return OkSuccess("Application deleted successfully");
    }

    [HttpGet("{applicationId:guid}/users")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApplicationUserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApplicationUserDto>>>> GetApplicationUsers(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var users = await mediator.Send(new GetApplicationUsersQuery(applicationId), cancellationToken);
        return OkResponse(users);
    }

    [HttpPost("{applicationId:guid}/users")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> AddUserToApplication(
        Guid applicationId,
        [FromBody] AddUserToApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddUserToApplicationCommand(
            applicationId,
            request.UserId,
            request.Role);

        await mediator.Send(command, cancellationToken);

        return OkSuccess("User added to application successfully");
    }
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CreateApplication)]
    [ProducesResponseType(typeof(ApiResponse<CreateApplicationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CreateApplicationResponse>>> CreateApplication(
        [FromBody] CreateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateApplicationCommand(
            request.Name,
            request.Description,
            request.OwnerId,
            request.DefaultLicensePackageId,
            request.SubscriptionPlanId,
            request.LogoUrl,
            request.AllowedRedirectUris,
            request.BackchannelLogoutUri);
        var created = await mediator.Send(command, cancellationToken);
        return CreatedResponse(nameof(GetApplication), created, new { applicationId = created.Id });
    }

    [HttpPut("{applicationId:guid}/logo")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<CreateApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CreateApplicationResponse>>> UpdateLogo(
        Guid applicationId,
        [FromBody] UpdateApplicationLogoRequest request,
        CancellationToken cancellationToken = default)
    {
        var updated = await mediator.Send(
            new UpdateApplicationLogoCommand(applicationId, request.LogoUrl),
            cancellationToken);

        return OkResponse(updated, "Updated");
    }
}
