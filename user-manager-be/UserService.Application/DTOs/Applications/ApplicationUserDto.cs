namespace UserService.Application.DTOs.Applications;

public sealed record ApplicationUserDto(
    Guid UserId,
    string Role,
    string? Email,
    string? UserName);

