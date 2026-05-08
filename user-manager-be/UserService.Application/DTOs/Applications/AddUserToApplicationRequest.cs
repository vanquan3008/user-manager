namespace UserService.Application.DTOs.Applications;

public record AddUserToApplicationRequest(
    Guid UserId,
    string? Role);
