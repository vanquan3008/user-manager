using Microsoft.AspNetCore.Identity;

namespace UserService.Infrastructure.Identity;

public sealed class AppRole : IdentityRole<Guid>
{
    public AppRole()
    {
        Id = Guid.NewGuid();
    }

    public AppRole(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
    }

    public AppRole(string roleName, string description) : this(roleName)
    {
        Description = description;
    }

    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
