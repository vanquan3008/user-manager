namespace UserService.Application.DTOs.Auth;

public sealed record VerifyEmailRequest(
    Guid? UserId,
    string Token
);
