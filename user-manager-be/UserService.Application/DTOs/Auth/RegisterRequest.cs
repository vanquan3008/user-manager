namespace UserService.Application.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? ClientId = null
);
