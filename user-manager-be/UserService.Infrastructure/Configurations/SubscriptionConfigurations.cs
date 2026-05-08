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
              .HasConversion(x => x.Amount, x => UserService.Domain.ValueObjects.Money.Create(x, "USD"))
              .HasPrecision(10, 2);

        builder.Property(x => x.YearlyPrice)
              .HasConversion(x => x.Amount, x => UserService.Domain.ValueObjects.Money.Create(x, "USD"))
              .HasPrecision(10, 2);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => new { x.ApplicationId, x.Name }).IsUnique();
        builder.HasIndex(x => x.IsActive);

        builder.Navigation(p => p.RoleAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class SubscriptionPlanRoleAssignmentConfiguration : IEntityTypeConfiguration<SubscriptionPlanRoleAssignment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanRoleAssignment> builder)
    {
        builder.ToTable("SubscriptionPlanRoleAssignments");
        builder.HasKey(x => new { x.SubscriptionPlanId, x.RoleId });
        
        builder.HasOne(x => x.SubscriptionPlan)
            .WithMany(p => p.RoleAssignments)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<UserService.Infrastructure.Identity.AppRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LicensePackageConfiguration : IEntityTypeConfiguration<LicensePackage>
{
    public void Configure(EntityTypeBuilder<LicensePackage> builder)
    {
        builder.ToTable("LicensePackages");
        builder.HasKey(x => x.Id);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_LicensePackages_SeatLimit_MinusOneOrMore", "\"SeatLimit\" >= -1");
        });

        builder.Property(x => x.Name).HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.PlanName.Create(x)).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FeatureJson).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.AssignedRole).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Application)
              .WithMany()
              .HasForeignKey(x => x.ApplicationId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany()
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired(false);

        builder.HasMany(p => p.Subscriptions)
              .WithOne(s => s.LicensePackage)
              .HasForeignKey(s => s.LicensePackageId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired(false);

        builder.Navigation(p => p.Subscriptions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.SubscriptionPlanId);
        builder.HasIndex(x => new { x.SubscriptionPlanId, x.Name }).IsUnique();
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

        builder.HasMany(x => x.Members)
              .WithOne(x => x.Subscription)
              .HasForeignKey(x => x.SubscriptionId)
              .OnDelete(DeleteBehavior.Cascade);

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
        builder.HasIndex(x => x.LicensePackageId);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => x.ExpiresAt);
    }
}

public sealed class SubscriptionMemberConfiguration : IEntityTypeConfiguration<SubscriptionMember>
{
    public void Configure(EntityTypeBuilder<SubscriptionMember> builder)
    {
        builder.ToTable("SubscriptionMembers");
        builder.HasKey(x => x.Id);

        builder.Ignore(x => x.IsAdmin);
        builder.Ignore(x => x.CanManageMembers);
        builder.Ignore(x => x.CanChangePaymentMethod);
        builder.Ignore(x => x.CanAccessAllApps);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RoleType)
              .WithMany()
              .HasForeignKey(x => x.RoleTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SubscriptionId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.SubscriptionId);
        builder.HasIndex(x => x.UserId);
    }
}
