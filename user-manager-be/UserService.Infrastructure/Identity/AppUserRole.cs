using Microsoft.AspNetCore.Identity;

namespace UserService.Infrastructure.Identity;
public sealed class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
