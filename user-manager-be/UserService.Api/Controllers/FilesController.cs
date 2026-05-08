using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Authorization;
using UserService.Application.Features.Files.Commands;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class FilesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Policy = AuthorizationPolicies.CreateOrManageApplications)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<UploadFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UploadFileDto>>> Upload(
        IFormFile file,
        [FromForm] string? folder = null)
    {
        if (file == null)
            return BadRequestResponse("File is required.");

        await using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            file.FileName,
            file.ContentType ?? string.Empty,
            stream,
            file.Length,
            folder);
        var result = await _mediator.Send(command, HttpContext.RequestAborted);
        return OkResponse(result, "Uploaded");
    }

    [HttpDelete]
    [Authorize(Policy = AuthorizationPolicies.ManageApplications)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        [FromQuery] string fileUrl)
    {
        await _mediator.Send(new DeleteFileCommand(fileUrl), HttpContext.RequestAborted);
        return OkResponse<object?>(null, "Deleted");
    }
}

