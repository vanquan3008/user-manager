using UserService.Domain.ValueObjects;

namespace UserService.Application.DTOs.Auth;

/// <summary>
/// Immutable request object for user registration.
/// </summary>
public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Guid? ApplicationId = null
);
