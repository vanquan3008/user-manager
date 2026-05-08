namespace UserService.Application.DTOs.Users;

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string Username,
    bool IsEmailVerified,
    bool IsActive,
    IEnumerable<string> Roles,
    DateTime CreatedAt);
