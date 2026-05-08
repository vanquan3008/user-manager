using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;

public interface IAuthServices
{
    Task<AuthResponse> RegisterUserAsync(
        string email,
        string password,
        string username,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginUserAsync(
        string email,
        string password,
        Guid? applicationId = null,
        string? returnUrl = null,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> SsoLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>Verify email khi user click link (chỉ cần token). Dùng cho GET /api/auth/verify-email?token=...</summary>
    Task<bool> VerifyEmailWithTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<bool> VerifyMagicLinkAsync(
        string token,
        string purpose,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> GetAuthResponseForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ResendMagicLinkAsync(
        string email,
        string purpose,
        CancellationToken cancellationToken = default);

    Task<bool> RequestPasswordlessLoginAsync(
        string email,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> VerifyMagicLinkAndAuthenticateAsync(
        string token,
        string purpose,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);
}
