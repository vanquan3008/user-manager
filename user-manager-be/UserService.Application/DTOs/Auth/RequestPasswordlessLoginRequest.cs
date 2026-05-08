namespace UserService.Application.DTOs.Auth;

public record RequestPasswordlessLoginRequest(
    string Email,
    string? ClientId = null
);
