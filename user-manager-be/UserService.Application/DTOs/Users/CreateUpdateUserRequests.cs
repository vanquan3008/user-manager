using UserService.Domain.Constants;

namespace UserService.Application.DTOs.Users;

public record CreateUserRequest(
    string Email,
    string Username,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? DisplayName = null
);

public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    string? DisplayName = null,
    string? AvatarUrl = null,
    string? Bio = null
);

public record BanUserRequest(string? Reason);

public record GrantSSOAppAccessRequest(Guid ApplicationId, string? Role = RoleNames.Admin);

public record UpgradeUserRoleRequest(string Role);
