using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AspNetUsers");
        
        builder.Property(x => x.RefreshToken).HasMaxLength(500);
        builder.Property(x => x.LastLoginIp).HasMaxLength(50);
        builder.Property(x => x.BanReason).HasMaxLength(500);

        builder.HasMany(x => x.UserRoles)
              .WithOne(x => x.User)
              .HasForeignKey(x => x.UserId)
              .IsRequired();

        builder.HasIndex(x => x.UserProfileId);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.NormalizedEmail);
    }
}

public sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("AspNetRoles");
        
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasMany(x => x.UserRoles)
              .WithOne(x => x.Role)
              .HasForeignKey(x => x.RoleId)
              .IsRequired();
    }
}

public sealed class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
{
    public void Configure(EntityTypeBuilder<AppUserRole> builder)
    {
        builder.ToTable("AspNetUserRoles");
        
        builder.HasKey(x => new { x.UserId, x.RoleId });
    }
}

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRoles");
        builder.HasKey(x => new { x.ApplicationId, x.RoleId });
        builder.Property(x => x.ApplicationId).IsRequired();
        builder.Property(x => x.RoleId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<AppApplication>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
