namespace UserService.Application.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password,
    string? ClientId = null,
    string? ReturnUrl = null
);
