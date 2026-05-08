namespace UserService.Application.DTOs.Auth;

public record VerifyMagicLinkRequest(
    string Token,
    string Purpose,
    string? ClientId = null
);
