using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserService.Api.Authorization;
using UserService.Api.Extensions;
using UserService.Application.Features.Subscriptions.Commands;
using UserService.Application.Features.Subscriptions.Queries;
using UserService.Domain.Constants;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SubscriptionsController(IMediator mediator) : ApiControllerBase
{
    [HttpPost("activate")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> Activate(
        [FromBody] ActivateSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new ActivateSubscriptionCommand(
            userId,
            request.ApplicationId,
            request.SubscriptionPlanId,
            request.SubscriptionType,
            request.ExpiresAt,
            request.MaxMembers);

        var subscription = await mediator.Send(command, cancellationToken);
        return OkResponse(subscription);
    }

    [HttpPost("activate-package")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> ActivatePackage(
        [FromBody] ActivateLicensePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new ActivateLicensePackageCommand(
            userId,
            request.ApplicationId,
            request.LicensePackageId,
            request.SubscriptionType,
            request.ExpiresAt,
            request.MaxMembers);

        var subscription = await mediator.Send(command, cancellationToken);
        return OkResponse(subscription);
    }

    [HttpPost("upgrade")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> Upgrade(
        [FromBody] UpgradeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new UpgradeSubscriptionCommand(
            userId,
            request.ApplicationId,
            request.NewSubscriptionPlanId,
            request.NewType,
            request.ExpiresAt);

        var subscription = await mediator.Send(command, cancellationToken);
        return OkResponse(subscription);
    }

    [HttpPost("upgrade-package")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> UpgradePackage(
        [FromBody] UpgradeLicensePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new UpgradeLicensePackageCommand(
            userId,
            request.ApplicationId,
            request.NewLicensePackageId,
            request.NewType,
            request.ExpiresAt);

        var subscription = await mediator.Send(command, cancellationToken);
        return OkResponse(subscription);
    }

    [HttpGet("plans")]
    [Authorize(Policy = AuthorizationPolicies.ViewApplications)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SubscriptionPlanCatalogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubscriptionPlanCatalogDto>>>> GetPlans(
        [FromQuery] Guid? applicationId,
        CancellationToken cancellationToken = default)
    {
        if (!applicationId.HasValue && !(User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.SuperAdmin)))
        {
            applicationId = User.TryGetTokenApplicationId();
            if (!applicationId.HasValue)
            {
                var ssoAppIds = User.GetAuthorizedSsoApplicationIds();
                if (ssoAppIds.Count == 1)
                {
                    applicationId = ssoAppIds.First();
                }
                else
                {
                    return BadRequestResponse("applicationId is required.", "APPLICATION_ID_REQUIRED");
                }
            }
        }

        var query = new GetSubscriptionPlansQuery(applicationId);
        var plans = await mediator.Send(query, cancellationToken);
        return OkResponse(plans);
    }

    /// <summary>
    /// Create a subscription plan (subplan) with an assigned application-scoped role.
    /// </summary>
    [HttpPost("plans")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanCatalogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanCatalogDto>>> CreatePlan(
        [FromBody] CreateSubscriptionPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSubscriptionPlanCommand(
            request.ApplicationId,
            request.Name,
            request.Description,
            request.MonthlyPrice,
            request.YearlyPrice,
            request.MaxUsers,
            request.MaxApiCalls,
            request.IncludeAnalytics,
            request.IncludeSupport,
            request.IncludeAdvancedSecurity,
            request.IncludeCustomDomain,
            request.IsActive,
            request.DisplayOrder,
            request.AssignedRoleIds,
            request.Icon);

        var created = await mediator.Send(command, cancellationToken);
        return OkResponse(created);
    }

    /// <summary>
    /// Update a subscription plan.
    /// </summary>
    [HttpPut("plans/{planId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanCatalogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubscriptionPlanCatalogDto>>> UpdatePlan(
        Guid planId,
        [FromBody] UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSubscriptionPlanCommand(
            planId,
            request.ApplicationId,
            request.Name,
            request.Description,
            request.MonthlyPrice,
            request.YearlyPrice,
            request.MaxUsers,
            request.MaxApiCalls,
            request.IncludeAnalytics,
            request.IncludeSupport,
            request.IncludeAdvancedSecurity,
            request.IncludeCustomDomain,
            request.IsActive,
            request.DisplayOrder,
            request.AssignedRoleIds,
            request.Icon);

        var updated = await mediator.Send(command, cancellationToken);
        return OkResponse(updated);
    }

    /// <summary>
    /// Get all license packages (packets) for an application.
    /// </summary>
    [HttpGet("packages")]
    [Authorize(Policy = AuthorizationPolicies.ViewApplications)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LicensePackageCatalogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LicensePackageCatalogDto>>>> GetPackages(
        [FromQuery] Guid applicationId,
        [FromQuery] Guid? subscriptionPlanId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLicensePackagesQuery(applicationId, subscriptionPlanId);
        var packages = await mediator.Send(query, cancellationToken);
        return OkResponse(packages);
    }

    /// <summary>
    /// Create a license package (packet) with an assigned application-scoped role.
    /// </summary>
    [HttpPost("packages")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<LicensePackageCatalogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LicensePackageCatalogDto>>> CreatePackage(
        [FromBody] CreateLicensePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLicensePackageCommand(
            request.ApplicationId,
            request.SubscriptionPlanId,
            request.Name,
            request.Description,
            request.Features,
            request.SeatLimit,
            request.IsActive,
            request.DisplayOrder,
            request.AssignedRole);

        var created = await mediator.Send(command, cancellationToken);
        return OkResponse(created);
    }

    /// <summary>
    /// Update a license package (packet).
    /// </summary>
    [HttpPut("packages/{packageId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<LicensePackageCatalogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LicensePackageCatalogDto>>> UpdatePackage(
        Guid packageId,
        [FromBody] UpdateLicensePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLicensePackageCommand(
            packageId,
            request.ApplicationId,
            request.Name,
            request.Description,
            request.Features,
            request.SeatLimit,
            request.IsActive,
            request.DisplayOrder,
            request.AssignedRole);

        var updated = await mediator.Send(command, cancellationToken);
        return OkResponse(updated);
    }

    /// <summary>
    /// Extend the expiration date of the active subscription.
    /// </summary>
    [HttpPost("extend")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> Extend(
        [FromBody] ExtendSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new ExtendSubscriptionCommand(
            userId,
            request.ApplicationId,
            request.NewExpiresAt);

        await mediator.Send(command, cancellationToken);
        return OkSuccess("Subscription extended successfully");
    }

    /// <summary>
    /// Cancel the active subscription of a user.
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> Cancel(
        [FromBody] CancelSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveRequestedUserId(request.UserId);
        var command = new CancelSubscriptionCommand(
            userId,
            request.ApplicationId);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Subscription cancelled successfully");
    }

    /// <summary>
    /// Get the currently active subscription for a user.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionDto?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SubscriptionDto?>>> GetActive(
        [FromQuery] Guid? userId,
        [FromQuery] Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var targetUserId = ResolveRequestedUserId(userId ?? Guid.Empty);
        var query = new GetActiveSubscriptionQuery(targetUserId, applicationId);
        var subscription = await mediator.Send(query, cancellationToken);
        return OkResponse(subscription);
    }

    /// <summary>
    /// Get subscription history for a user.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriptionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionDto>>>> GetHistory(
        [FromQuery] Guid? userId,
        [FromQuery] Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var targetUserId = ResolveRequestedUserId(userId ?? Guid.Empty);
        var query = new GetSubscriptionHistoryQuery(targetUserId, applicationId);
        var history = await mediator.Send(query, cancellationToken);
        return OkResponse(history);
    }

    [HttpPost("members")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> AddMember(
        [FromBody] AddSubscriptionMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddSubscriptionMemberCommand(
            request.SubscriptionId,
            request.ApplicationId,
            request.UserId,
            request.RoleTypeId);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Member added to subscription successfully");
    }

    [HttpDelete("members")]
    [Authorize(Policy = AuthorizationPolicies.ManageUsers)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> RemoveMember(
        [FromBody] RemoveSubscriptionMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveSubscriptionMemberCommand(
            request.SubscriptionId,
            request.ApplicationId,
            request.UserId);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Member removed from subscription successfully");
    }

    private Guid ResolveRequestedUserId(Guid? requestedUserId)
    {
        var currentUserId = User.GetRequiredCurrentUserId();
        var isAdmin = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.SuperAdmin);

        if (!requestedUserId.HasValue || requestedUserId.Value == Guid.Empty)
            return currentUserId;

        if (requestedUserId.Value != currentUserId && !isAdmin)
            throw new UnauthorizedAccessException("You are not allowed to access another user's subscription.");

        return requestedUserId.Value;
    }
}

