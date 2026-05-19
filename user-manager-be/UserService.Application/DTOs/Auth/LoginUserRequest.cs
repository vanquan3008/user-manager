namespace UserService.Application.DTOs.Auth;

/// <summary>
/// Immutable request object for user login.
/// </summary>
public sealed record LoginUserRequest(
    string Email,
    string Password,
    Guid? ApplicationId = null,
    string? ReturnUrl = null
);
