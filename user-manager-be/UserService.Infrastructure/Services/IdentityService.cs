using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserService.Application.Interfaces.Services;
using UserService.Application.Mapping;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Identity;
using static UserService.Domain.Constants.EmailPurpose;

namespace UserService.Infrastructure.Services;
public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IMagicLinkService _magicLinkService;
    private readonly IUserRepository _userRepository;

    public IdentityService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> signInManager,
        IMagicLinkService magicLinkService,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _magicLinkService = magicLinkService;
        _userRepository = userRepository;
    }

    public async Task<(bool Succeeded, Guid? UserId, IEnumerable<string> Errors)> RegisterAsync(string email,string password, string username, CancellationToken cancellationToken = default)
    {
        var user = new AppUser(username, email);
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (false, null, createResult.Errors.Select(e => e.Description));
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, Domain.Constants.RoleNames.User);
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return (false, null, addRoleResult.Errors.Select(e => e.Description));
        }

        return (true, user.Id, Array.Empty<string>());
    }


    public async Task<(bool Succeeded, IdentityUserDto? User, IEnumerable<string> Errors)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, null, new[] { "Invalid email or password" });
        }

        if (!user.IsActive)
        {
            return (false, null, new[] { "Account is not active" });
        }

        if (user.IsBanned)
        {
            return (false, null, new[] { $"Account is banned. Reason: {user.BanReason}" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return (false, null, new[] { "Account is locked out" });
            }

            return (false, null, new[] { "Invalid email or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var domainUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        var userDto = IdentityUserMapper.ToDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.IsActive,
            user.IsBanned,
            user.BanReason ?? string.Empty,
            user.CreatedAt,
            user.UserProfileId,
            domainUser?.IsSSOAccount ?? false);
        
        return (true, userDto, Enumerable.Empty<string>());
    }

    public async Task<bool> VerifyEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var (isValid, validatedUserId) = await _magicLinkService.ValidateMagicLinkAsync(
            token, 
            MagicLinkPurpose.EmailVerification, 
            cancellationToken);

        if (!isValid || validatedUserId != userId)
            return false;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;

        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
            return false;

        var (isValid, validatedUserId) = await _magicLinkService.ValidateMagicLinkAsync(
            token, 
            MagicLinkPurpose.PasswordReset, 
            cancellationToken);

        if (!isValid || validatedUserId != user.Id)
            return false;

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        return result.Succeeded;
    }

    public async Task<IdentityUserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;
        
        var domainUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return IdentityUserMapper.ToDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.IsActive,
            user.IsBanned,
            user.BanReason ?? string.Empty,
            user.CreatedAt,
            user.UserProfileId,
            domainUser?.IsSSOAccount ?? false);
    }

    public async Task<IdentityUserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;
        var domainUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        return IdentityUserMapper.ToDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.IsActive,
            user.IsBanned,
            user.BanReason ?? string.Empty,
            user.CreatedAt,
            user.UserProfileId,
            domainUser?.IsSSOAccount ?? false);
    }

    public async Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;

        user.RefreshToken = HashRefreshToken(refreshToken);
        user.RefreshTokenExpiryTime = expiryTime;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<IdentityUserDto?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hashedRefreshToken = HashRefreshToken(refreshToken);
        var user = await _userManager.Users
            .FirstOrDefaultAsync(
                u => u.RefreshToken == hashedRefreshToken || u.RefreshToken == refreshToken,
                cancellationToken);

        if (user == null || !user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value < DateTime.UtcNow)
            return null;

        var domainUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        return IdentityUserMapper.ToDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.IsActive,
            user.IsBanned,
            user.BanReason ?? string.Empty,
            user.CreatedAt,
            user.UserProfileId,
            domainUser?.IsSSOAccount ?? false);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    public async Task<bool> UpdateUserProfileIdAsync(Guid userId, Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return false;

        user.UserProfileId = userProfileId;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded;
    }

    public async Task<bool> AddToRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveFromRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<(IReadOnlyList<IdentityUserDto> Items, int TotalCount)> GetUsersAsync(int pageIndex, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u => (u.Email != null && u.Email.Contains(s)) || (u.UserName != null && u.UserName.Contains(s)));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var list = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var userIds = list.Select(u => u.Id).ToList();
        var domainUsers = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
        var domainUserDict = domainUsers.ToDictionary(du => du.Id);
        var items = list.Select(u => {
            var du = domainUserDict.GetValueOrDefault(u.Id);
            return IdentityUserMapper.ToDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.Email ?? string.Empty,
                u.EmailConfirmed,
                u.IsActive,
                u.IsBanned,
                u.BanReason ?? string.Empty,
                u.CreatedAt,
                u.UserProfileId,
                du?.IsSSOAccount ?? false);
        }).ToList();
        
        return (items, totalCount);
    }

    public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            return (false, ["User not found"]);

        var result = await _userManager.DeleteAsync(user);
        
        return result.Succeeded 
            ? (true, Enumerable.Empty<string>()) 
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> BanUserAsync(Guid userId, string? reason, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, new[] { "User not found" });

        var r = string.IsNullOrWhiteSpace(reason) ? "Banned by administrator" : reason.Trim();
        if (r.Length > 500)
            r = r[..500];

        user.IsBanned = true;
        user.BannedAt = DateTime.UtcNow;
        user.BanReason = r;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, Enumerable.Empty<string>())
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, new[] { "User not found" });

        user.IsBanned = false;
        user.BannedAt = null;
        user.BanReason = null;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, Enumerable.Empty<string>())
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<Guid?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return null;
        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.Id;
    }

    public Task<(bool Succeeded, IEnumerable<string> Errors)> UnbanUserAsync(Guid userId, string? reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
