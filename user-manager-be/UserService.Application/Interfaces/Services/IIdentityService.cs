namespace UserService.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<(bool Succeeded, Guid? UserId, IEnumerable<string> Errors)> RegisterAsync(
        string email, 
        string password, 
        string username,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IdentityUserDto? User, IEnumerable<string> Errors)> LoginAsync(
        string email, 
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken = default);
    Task<IdentityUserDto?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserProfileIdAsync(Guid userId, Guid userProfileId, CancellationToken cancellationToken = default);
    Task<bool> AddToRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<IdentityUserDto> Items, int TotalCount)> GetUsersAsync(int pageIndex, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> BanUserAsync(Guid userId, string? reason, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UnbanUserAsync(Guid userId, string? reason, CancellationToken cancellationToken = default);
    Task<Guid?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
