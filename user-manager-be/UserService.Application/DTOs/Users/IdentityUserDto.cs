namespace UserService.Application.DTOs.Users;

public record IdentityUserDto(
    Guid Id,
    string UserName,
    string Email,
    bool EmailConfirmed,
    bool IsActive,
    bool IsBanned,
    string? BanReason,
    DateTime CreatedAt,
    Guid? UserProfileId,
    bool IsSSOAccount = false,
    bool TwoFactorEnabled = false,
    DateTime? LastLoginAt = null,
    DateTimeOffset? LockoutEnd = null
);

/// <summary>Outcome of a password verification. <see cref="ErrorCode"/> is one of <c>AuthErrorCodes</c>.</summary>
public record PasswordLoginResult(bool Succeeded, IdentityUserDto? User, string? ErrorCode, string? Message)
{
    public static PasswordLoginResult Success(IdentityUserDto user) => new(true, user, null, null);
    public static PasswordLoginResult Failure(string code, string message) => new(false, null, code, message);
}

public record UserWithRolesDto(IdentityUserDto User, IList<string> Roles);
