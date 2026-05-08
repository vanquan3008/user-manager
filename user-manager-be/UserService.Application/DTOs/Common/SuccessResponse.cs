namespace UserService.Application.DTOs.Common;

public record SuccessResponse(
    bool Success,
    string Message
);
