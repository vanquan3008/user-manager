using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Configurations;

public sealed class AppApplicationConfiguration : IEntityTypeConfiguration<AppApplication>
{
    public void Configure(EntityTypeBuilder<AppApplication> builder)
    {
        builder.ToTable("AppApplications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
              .HasConversion(x => x.Value, x => AppName.Create(x))
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.Description)
              .HasConversion(x => x.Value, x => AppDescription.Create(x))
              .HasMaxLength(1000);

        builder.Property(x => x.ApiKey)
              .HasConversion(x => x.Value, x => ApiKey.Create(x))
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.ApiSecretHash)
              .HasConversion(x => x.Value, x => ApiSecretHash.Create(x))
              .IsRequired()
              .HasColumnName("ApiSecret")
              .HasMaxLength(500);

        builder.Property(x => x.WebhookUrl)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UrlValue.Create(x) : null)
              .HasMaxLength(500);
        builder.Property(x => x.LogoUrl)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UrlValue.Create(x) : null)
              .HasMaxLength(500);

        var allowedRedirectUris = builder.Property(x => x.AllowedRedirectUris)
              .HasConversion(
                  v => v != null && v.Any() ? string.Join(",", v.Select(u => u.Value)) : null,
                  v => !string.IsNullOrEmpty(v)
                      ? new HashSet<UrlValue>(v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => UrlValue.Create(s)))
                      : new HashSet<UrlValue>())
              .HasMaxLength(2000);

        allowedRedirectUris.Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<UrlValue>>(
            (a, b) => ReferenceEquals(a, b) || (a != null && b != null && a.Count == b.Count && a.All(b.Contains)),
            v => v == null ? 0 : v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
            v => v == null ? new HashSet<UrlValue>() : new HashSet<UrlValue>(v)));

        builder.Property(x => x.BackchannelLogoutUri)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UrlValue.Create(x) : null)
              .HasMaxLength(1000);
        builder.HasOne(x => x.Owner)
              .WithMany()
              .HasForeignKey(x => x.OwnerId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DefaultLicensePackage)
              .WithMany()
              .HasForeignKey(x => x.DefaultLicensePackageId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany()
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Users)
              .WithOne(x => x.Application)
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);
        
        builder.Navigation(x => x.Users).HasField("_users");

        // Indexes
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.DefaultLicensePackageId);
        builder.HasIndex(x => x.ApiKey).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
    }
}

public sealed class UserApplicationConfiguration : IEntityTypeConfiguration<UserApplication>
{
    public void Configure(EntityTypeBuilder<UserApplication> builder)
    {
        builder.ToTable("UserApplications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
              .IsRequired()
              .HasMaxLength(50);
              
        builder.Property(x => x.LicenseAssignedRole)
              .HasMaxLength(100);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LicensePackage)
              .WithMany()
              .HasForeignKey(x => x.LicensePackageId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.ApplicationId }).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.LicensePackageId);
        builder.HasIndex(x => x.IsActive);
    }
}
