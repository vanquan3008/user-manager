using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Code).HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.PermissionCode.Create(x)).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(100);
        builder.Property(p => p.IsSystemPermission).HasDefaultValue(false);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.Scope).HasMaxLength(100);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.IsActive);
        
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).ValueGeneratedNever();
        builder.Property(rp => rp.RoleId).IsRequired();
        builder.Property(rp => rp.PermissionId).IsRequired();
        builder.Property(rp => rp.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Match soft-delete query filters on required principals to avoid EF warnings.
        builder.HasQueryFilter(rp => rp.Permission != null && !rp.Permission.IsDeleted);
        
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        builder.HasIndex(rp => rp.RoleId);
        builder.HasIndex(rp => rp.PermissionId);
        
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkToken>
{
    public void Configure(EntityTypeBuilder<MagicLinkToken> builder)
    {
        builder.ToTable("MagicLinkTokens");
        builder.HasKey(x => x.Id);

        // Match soft-delete query filters on required principals to avoid EF warnings.
        builder.HasQueryFilter(x => !x.User.IsDeleted);

        builder.Property(x => x.Token)
              .HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.TokenValue.Create(x))
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.Purpose)
              .IsRequired()
              .HasMaxLength(50);

        builder.Property(x => x.Email)
              .HasMaxLength(256);

        builder.Property(x => x.IpAddress)
              .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
              .HasMaxLength(500);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

        builder.HasIndex(x => new { x.Token, x.Purpose, x.IsUsed });
        builder.HasIndex(x => new { x.UserId, x.Purpose });
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => x.CreatedAt);
    }
}
