using UserService.Application.DTOs.Users;

namespace UserService.Application.DTOs.Auth;

public record ApplicationInfoDto(
    Guid Id,
    string Name,
    string? Description);

public record AuthResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    UserDto User,
    bool RequiresEmailVerification = false,
    IReadOnlyList<ApplicationInfoDto>? Applications = null
);
