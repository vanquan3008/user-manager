namespace UserService.Application.Interfaces.Services;

public sealed record UserStatistics(int Total, int Active, int Banned, int Locked, int MfaEnabled, int EmailVerified, int NewSince, int SignedInSince);

public interface IIdentityService
{
    Task<(bool Succeeded, Guid? UserId, IEnumerable<string> Errors)> RegisterAsync(
        string email, 
        string password, 
        string username,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies credentials with lockout. Account state (inactive/banned) is only disclosed after the
    /// password has been verified, so the endpoint cannot be used to probe account status.
    /// </summary>
    Task<PasswordLoginResult> LoginAsync(
        string email, 
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>Records a completed sign-in (after MFA when required).</summary>
    Task RecordSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> VerifyEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsSignInAllowedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserStatistics> GetUserStatisticsAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserProfileIdAsync(Guid userId, Guid userProfileId, CancellationToken cancellationToken = default);
    Task<bool> AddToRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<IdentityUserDto> Items, int TotalCount)> GetUsersAsync(int pageIndex, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> BanUserAsync(Guid userId, string? reason, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
