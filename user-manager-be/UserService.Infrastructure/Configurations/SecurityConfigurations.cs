using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RefreshTokenHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PreviousRefreshTokenHash).HasMaxLength(128);
        builder.Property(x => x.SessionKind).IsRequired().HasMaxLength(32);
        builder.Property(x => x.AuthenticationMethod).IsRequired().HasMaxLength(32);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.RevokedReason).HasMaxLength(100);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => x.PreviousRefreshTokenHash);
        builder.HasIndex(x => new { x.UserId, x.RevokedAt, x.ExpiresAt });
        builder.HasIndex(x => x.ExpiresAt);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ActorEmail).HasMaxLength(256);
        builder.Property(x => x.TargetType).HasMaxLength(50);
        builder.Property(x => x.TargetId).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Details).HasMaxLength(4000);

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.EventType, x.OccurredAt });
        builder.HasIndex(x => new { x.Category, x.OccurredAt });
        builder.HasIndex(x => new { x.ActorUserId, x.OccurredAt });
        builder.HasIndex(x => new { x.TargetUserId, x.OccurredAt });
    }
}

public sealed class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("SigningKeys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.KeyId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Algorithm).IsRequired().HasMaxLength(16);
        builder.Property(x => x.PublicKeyPem).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.ProtectedPrivateKey).IsRequired();
        builder.Ignore(x => x.IsActive);
        builder.HasIndex(x => x.KeyId).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}
