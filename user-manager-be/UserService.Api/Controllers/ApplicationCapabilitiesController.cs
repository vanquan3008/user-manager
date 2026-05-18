using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Extensions;
using UserService.Application.DTOs.Authorization;
using UserService.Application.Interfaces.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/applications/{applicationId:guid}/me/capabilities")]
[Produces("application/json")]
public sealed class ApplicationCapabilitiesController(ICapabilityService capabilityService) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ApplicationCapabilitiesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ApplicationCapabilitiesDto>>> GetCapabilities(
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetRequiredCurrentUserId();
        var capabilities = await capabilityService.GetCapabilitiesAsync(userId, applicationId, cancellationToken);
        return OkResponse(capabilities);
    }
}
