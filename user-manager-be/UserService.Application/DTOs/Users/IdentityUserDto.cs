namespace UserService.Application.DTOs.Users;

public record IdentityUserDto(
    Guid Id,
    string UserName,
    string Email,
    bool EmailConfirmed,
    bool IsActive,
    bool IsBanned,
    string? BanReason,
    string? RefreshToken,
    DateTime? RefreshTokenExpiryTime,
    DateTime CreatedAt,
    Guid? UserProfileId,
    bool IsSSOAccount = false
);

public record UserWithRolesDto(IdentityUserDto User, IList<string> Roles);
