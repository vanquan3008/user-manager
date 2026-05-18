namespace UserService.Application.DTOs.Auth;

public record VerifyMagicLinkRequest(
    string Token,
    string Purpose,
    Guid? ApplicationId = null,
    string? ClientId = null
);
