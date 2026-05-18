using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Configurations;

public sealed class UserOAuthConsentConfiguration : IEntityTypeConfiguration<UserOAuthConsent>
{
    public void Configure(EntityTypeBuilder<UserOAuthConsent> builder)
    {
        builder.ToTable("UserOAuthConsents");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(typeof(UserOAuthConsentScope), "_scopes")
              .WithOne(nameof(UserOAuthConsentScope.Consent))
              .HasForeignKey(nameof(UserOAuthConsentScope.ConsentId))
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.ApplicationId }).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.IsActive);
    }
}

public sealed class UserOAuthConsentScopeConfiguration : IEntityTypeConfiguration<UserOAuthConsentScope>
{
    public void Configure(EntityTypeBuilder<UserOAuthConsentScope> builder)
    {
        builder.ToTable("UserOAuthConsentScopes");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Consent)
              .WithMany()
              .HasForeignKey(x => x.ConsentId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
              .WithMany()
              .HasForeignKey(x => x.PermissionId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ConsentId, x.PermissionId }).IsUnique();
        builder.HasIndex(x => x.ConsentId);
        builder.HasIndex(x => x.PermissionId);
    }
}

public sealed class OAuthAuthorizationCodeConfiguration : IEntityTypeConfiguration<OAuthAuthorizationCode>
{
    public void Configure(EntityTypeBuilder<OAuthAuthorizationCode> builder)
    {
        builder.ToTable("OAuthAuthorizationCodes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(200);
        builder.Property(x => x.RedirectUri).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Scopes).HasMaxLength(1000);
        builder.Property(x => x.CodeChallenge).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CodeChallengeMethod).IsRequired().HasMaxLength(20);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => new { x.ApplicationId, x.UserId });
    }
}

public sealed class OAuthRefreshTokenConfiguration : IEntityTypeConfiguration<OAuthRefreshToken>
{
    public void Configure(EntityTypeBuilder<OAuthRefreshToken> builder)
    {
        builder.ToTable("OAuthRefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.TokenValue.Create(x)).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Scopes).HasMaxLength(1000);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.ApplicationId });
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => x.RevokedAtUtc);
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
