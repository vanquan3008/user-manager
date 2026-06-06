using System.Text.Json.Serialization;

namespace UserService.Api.Contracts.Auth;

public sealed record RegisterApiRequest(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    [property: JsonPropertyName("dateOfBirth")] DateOnly DateOfBirth,
    string? ClientId = null
);

public sealed record LoginApiRequest(
    string Email,
    string Password,
    string? ClientId = null,
    string? ReturnUrl = null
);

public sealed record RequestPasswordlessLoginApiRequest(
    string Email,
    string? ClientId = null
);

public sealed record VerifyMagicLinkApiRequest(
    string Token,
    string Purpose,
    string? ClientId = null
);

public sealed record VerifyEmailApiRequest(
    Guid? UserId,
    string Token
);

public sealed record ResendMagicLinkApiRequest(
    string Email,
    string Purpose
);

public sealed record RefreshTokenApiRequest(
    string? RefreshToken = null
);

public sealed record RequestPasswordResetApiRequest(
    string Email
);

public sealed record ResetPasswordApiRequest(
    string Email,
    string Token,
    string NewPassword
);

public sealed record ChangePasswordApiRequest(
    string CurrentPassword,
    string NewPassword
);
