using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(x => x.Id);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_SubscriptionPlans_MonthlyPrice_NonNegative", "\"MonthlyPrice\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlans_YearlyPrice_NonNegative", "\"YearlyPrice\" >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlans_MaxUsers_MinusOneOrMore", "\"MaxUsers\" >= -1");
            t.HasCheckConstraint("CK_SubscriptionPlans_MaxApiCalls_NullOrNonNegative", "\"MaxApiCalls\" IS NULL OR \"MaxApiCalls\" >= 0");
        });

        builder.HasOne<AppApplication>()
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Name)
              .HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.PlanName.Create(x))
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.Description)
              .HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.PlanDescription.Create(x))
              .IsRequired()
              .HasMaxLength(1000);

        builder.Property(x => x.Icon)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UserService.Domain.ValueObjects.UrlValue.Create(x) : null)
              .HasMaxLength(500);

        builder.Property(x => x.MonthlyPrice)
              .HasConversion(x => x.Amount, x => UserService.Domain.ValueObjects.Money.Create(x, "VND"))
              .HasPrecision(18, 2);

        builder.Property(x => x.YearlyPrice)
              .HasConversion(x => x.Amount, x => UserService.Domain.ValueObjects.Money.Create(x, "VND"))
              .HasPrecision(18, 2);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => new { x.ApplicationId, x.Name }).IsUnique();
        builder.HasIndex(x => x.IsActive);

    }
}

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Subscriptions_MaxMembers_Positive", "\"MaxMembers\" > 0");
        });

        builder.Property(x => x.EntitlementSnapshot)
              .HasField("_entitlementSnapshot")
              .HasConversion(new EntitlementSnapshotConverter())
              .HasColumnName("EntitlementSnapshotJson");

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany()
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired(false);

        builder.HasIndex(x => new { x.UserId, x.ApplicationId, x.IsActive });
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => x.ExpiresAt);
    }
}

public sealed class SubscriptionPlanPermissionConfiguration : IEntityTypeConfiguration<SubscriptionPlanPermission>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanPermission> builder)
    {
        builder.ToTable("SubscriptionPlanPermissions");
        builder.HasKey(x => new { x.SubscriptionPlanId, x.PermissionId });

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany(x => x.Permissions)
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
              .WithMany()
              .HasForeignKey(x => x.PermissionId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PermissionId);
    }
}
