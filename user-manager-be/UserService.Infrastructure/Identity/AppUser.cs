using Microsoft.AspNetCore.Identity;
namespace UserService.Infrastructure.Identity;
public sealed class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public AppUser(string userName, string email) : this()
    {
        UserName = userName;
        Email = email;
        NormalizedUserName = userName.ToUpperInvariant();
        NormalizedEmail = email.ToUpperInvariant();
    }
    public Guid? UserProfileId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }
    /// <summary>Last accepted TOTP time-step; codes at or before it are rejected (replay protection).</summary>
    public long? LastTotpTimeStep { get; set; }
    public DateTime? MfaEnabledAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
