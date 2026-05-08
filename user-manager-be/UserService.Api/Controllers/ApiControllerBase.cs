using Microsoft.AspNetCore.Mvc;

namespace UserService.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected OkObjectResult OkResponse<T>(T data, string message = "OK") =>
        Ok(new ApiResponse<T>(true, data, message));

    protected CreatedAtActionResult CreatedResponse<T>(
        string actionName,
        T data,
        object? routeValues = null,
        string message = "Created") =>
        CreatedAtAction(actionName, routeValues, new ApiResponse<T>(true, data, message));

    protected OkObjectResult OkSuccess(string message) =>
        OkResponse(new SuccessResponse(true, message));

    protected BadRequestObjectResult BadRequestResponse(string message, string code = "BAD_REQUEST") =>
        BadRequest(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message)));

    protected BadRequestObjectResult BadRequestResponse(
        string message,
        string code,
        IDictionary<string, string[]>? errors) =>
        BadRequest(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message, errors)));

    protected UnauthorizedObjectResult UnauthorizedResponse(string message, string code = "UNAUTHORIZED") =>
        Unauthorized(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message)));

    protected ConflictObjectResult ConflictResponse(string message, string code = "CONFLICT") =>
        Conflict(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message)));

    protected NotFoundObjectResult NotFoundResponse(string message, string code = "NOT_FOUND") =>
        NotFound(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message)));

    protected NotFoundObjectResult NotFoundResponse(
        string message,
        string code,
        IDictionary<string, string[]>? errors) =>
        NotFound(new ApiResponse<object>(false, null, message, new ErrorDetail(code, message, errors)));
}
