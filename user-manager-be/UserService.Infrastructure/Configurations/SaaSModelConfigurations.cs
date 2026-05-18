using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;

namespace UserService.Infrastructure.Configurations;

public sealed class ApplicationMembershipConfiguration : IEntityTypeConfiguration<ApplicationMembership>
{
    public void Configure(EntityTypeBuilder<ApplicationMembership> builder)
    {
        builder.ToTable("ApplicationMemberships");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
              .HasConversion(
                  x => x.ToString(),
                  x => Enum.Parse<MembershipStatus>(x))
              .IsRequired()
              .HasMaxLength(40);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppRole>()
              .WithMany()
              .HasForeignKey(x => x.RoleId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ApplicationId, x.UserId })
              .IsUnique()
              .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsActive);
    }
}

public sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.Name)
              .IsRequired()
              .HasMaxLength(200);

        builder.Property(x => x.Description)
              .HasMaxLength(1000);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

public sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("PlanFeatures");
        builder.HasKey(x => new { x.SubscriptionPlanId, x.FeatureId });

        builder.Property(x => x.LimitsJson)
              .HasMaxLength(8000);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany(x => x.Features)
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Feature)
              .WithMany()
              .HasForeignKey(x => x.FeatureId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FeatureId);
        builder.HasIndex(x => x.IsEnabled);
    }
}

public sealed class ApplicationSubscriptionConfiguration : IEntityTypeConfiguration<ApplicationSubscription>
{
    public void Configure(EntityTypeBuilder<ApplicationSubscription> builder)
    {
        builder.ToTable("ApplicationSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
              .HasConversion(
                  x => x.ToString(),
                  x => Enum.Parse<ApplicationSubscriptionStatus>(x))
              .IsRequired()
              .HasMaxLength(40);

        builder.Property(x => x.EntitlementSnapshot)
              .HasConversion(new EntitlementSnapshotConverter())
              .HasColumnName("EntitlementSnapshotJson");

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany()
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.ApplicationId)
              .IsUnique()
              .HasDatabaseName("IX_ApplicationSubscriptions_ApplicationId_Active")
              .HasFilter("\"Status\" IN ('Trialing', 'Active', 'PastDue') AND \"CancelledAt\" IS NULL");
        builder.HasIndex(x => x.SubscriptionPlanId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.ApplicationId, x.Status });
        builder.HasIndex(x => new { x.ApplicationId, x.CancelledAt });
    }
}
