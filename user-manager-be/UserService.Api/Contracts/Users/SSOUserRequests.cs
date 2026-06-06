using System.Text.Json.Serialization;

namespace UserService.Api.Contracts.Users;

public sealed record CreateSSOUserApiRequest(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    [property: JsonPropertyName("dateOfBirth")] DateOnly? DateOfBirth
);

public sealed record UpgradeUserRoleApiRequest(
    string Role
);

public sealed record GrantSSOAppAccessApiRequest(
    Guid ApplicationId,
    string? Role = null
);

public sealed record BanUserApiRequest(
    string? Reason
);
