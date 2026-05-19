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
    [Authorize(Policy = AuthorizationPolicies.ActivateSubscription)]
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

    [HttpPost("upgrade")]
    [Authorize(Policy = AuthorizationPolicies.UpgradeSubscription)]
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

    [HttpGet("plans")]
    [Authorize(Policy = AuthorizationPolicies.ViewSubscriptions)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SubscriptionPlanCatalogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubscriptionPlanCatalogDto>>>> GetPlans(
        [FromQuery] Guid? applicationId,
        CancellationToken cancellationToken = default)
    {
        if (!applicationId.HasValue && !User.IsInRole(RoleNames.SuperAdmin))
        {
            applicationId = User.TryGetTokenApplicationId();
            if (!applicationId.HasValue)
            {
                var ssoAppIds = User.GetAuthorizedSSOApplicationIds();
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
    /// Create a subscription plan (subplan). User roles are assigned through application memberships.
    /// </summary>
    [HttpPost("plans")]
    [Authorize(Policy = AuthorizationPolicies.ManageSubscriptions)]
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
            request.Features,
            request.Permissions,
            request.IsActive,
            request.DisplayOrder,
            request.Icon);

        var created = await mediator.Send(command, cancellationToken);
        return OkResponse(created);
    }

    /// <summary>
    /// Update a subscription plan.
    /// </summary>
    [HttpPut("plans/{planId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageSubscriptions)]
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
            request.Features,
            request.Permissions,
            request.IsActive,
            request.DisplayOrder,
            request.Icon);

        var updated = await mediator.Send(command, cancellationToken);
        return OkResponse(updated);
    }

    /// <summary>
    /// Deactivate a subscription plan (soft-delete).
    /// </summary>
    [HttpDelete("plans/{planId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ManageSubscriptions)]
    [ProducesResponseType(typeof(ApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SuccessResponse>>> DeletePlan(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateSubscriptionPlanCommand(planId);
        await mediator.Send(command, cancellationToken);
        return OkSuccess("Subscription plan deactivated successfully");
    }

    /// <summary>
    /// Extend the expiration date of the active subscription.
    /// </summary>
    [HttpPost("extend")]
    [Authorize(Policy = AuthorizationPolicies.ExtendSubscription)]
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
    [Authorize(Policy = AuthorizationPolicies.CancelSubscription)]
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

