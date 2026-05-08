using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Configurations;

public sealed class FamilyPlanConfiguration : IEntityTypeConfiguration<FamilyPlan>
{
    public void Configure(EntityTypeBuilder<FamilyPlan> builder)
    {
        builder.ToTable("FamilyPlans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FamilyName)
              .HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.PlanName.Create(x))
              .IsRequired()
              .HasMaxLength(200);

        builder.Property(x => x.FamilyDescription)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UserService.Domain.ValueObjects.PlanDescription.Create(x) : null)
              .HasMaxLength(1000);

        builder.Property(x => x.FamilyIcon)
              .HasConversion(x => x != null ? x.Value : null, x => x != null ? UserService.Domain.ValueObjects.UrlValue.Create(x) : null)
              .HasMaxLength(500);

        builder.Property(x => x.InviteCode)
              .HasConversion(x => x.Value, x => UserService.Domain.ValueObjects.InviteCode.Create(x))
              .IsRequired()
              .HasMaxLength(20);

        builder.Property(x => x.SharedStorageMB)
              .HasConversion(x => x.Megabytes, x => UserService.Domain.ValueObjects.StorageQuota.Create(x))
              .HasColumnName("SharedStorageMB");

        builder.Property(x => x.UsedStorageMB)
              .HasConversion(x => x.Megabytes, x => UserService.Domain.ValueObjects.StorageQuota.Create(x))
              .HasColumnName("UsedStorageMB");

        // Relationships
        builder.HasOne(x => x.Owner)
              .WithMany()
              .HasForeignKey(x => x.OwnerId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SubscriptionPlan>()
              .WithMany()
              .HasForeignKey(x => x.FamilySubscriptionPlanId)
              .OnDelete(DeleteBehavior.SetNull);

        // Members collection
        builder.HasMany(x => x.Members)
              .WithOne(x => x.FamilyPlan)
              .HasForeignKey(x => x.FamilyPlanId)
              .OnDelete(DeleteBehavior.Cascade);

        // Shared applications collection
        builder.OwnsMany(x => x.SharedApplications, nav =>
        {
            nav.ToTable("FamilySharedApplications");
            nav.WithOwner().HasForeignKey("FamilyPlanId");
            nav.HasKey("Id");

            nav.HasOne(sa => sa.Application)
               .WithMany()
               .HasForeignKey(sa => sa.ApplicationId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.OwnsMany(sa => sa.AllowedMembers, am =>
            {
                am.ToTable("FamilySharedApplicationAllowedMembers");
                am.WithOwner().HasForeignKey("SharedApplicationId");
                am.HasKey("SharedApplicationId", "FamilyMemberId");
                am.HasOne(x => x.FamilyMember)
                  .WithMany()
                  .HasForeignKey(x => x.FamilyMemberId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        });

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.InviteCode).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public sealed class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("FamilyMembers");
        builder.HasKey(x => x.Id);

        // Match soft-delete query filters on required principals to avoid EF warnings.
        builder.HasQueryFilter(x => !x.FamilyPlan.IsDeleted && !x.User.IsDeleted);

        builder.Ignore(x => x.IsAdmin);

        builder.HasOne(x => x.RoleType)
              .WithMany()
              .HasForeignKey(x => x.RoleTypeId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.FamilyPlanId }).IsUnique();
        builder.HasIndex(x => x.FamilyPlanId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsAdmin);
    }
}

public sealed class FamilySubscriptionConfiguration : IEntityTypeConfiguration<FamilySubscription>
{
    public void Configure(EntityTypeBuilder<FamilySubscription> builder)
    {
        builder.ToTable("FamilySubscriptions");
        builder.HasKey(x => x.Id);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_FamilySubscriptions_PricePerMonth_NonNegative", "\"PricePerMonth\" >= 0");
            t.HasCheckConstraint("CK_FamilySubscriptions_PricePerYear_NonNegative", "\"PricePerYear\" >= 0");
        });

        builder.Property(x => x.BillingCycle)
              .IsRequired()
              .HasMaxLength(20);

        builder.Property(x => x.PaymentMethodId)
              .HasMaxLength(200);

        builder.Property(x => x.PricePerMonth)
              .HasConversion(x => x != null ? (decimal?)x.Amount : null, x => x != null ? UserService.Domain.ValueObjects.Money.Create(x.Value, "USD") : null)
              .HasPrecision(10, 2);

        builder.Property(x => x.PricePerYear)
              .HasConversion(x => x != null ? (decimal?)x.Amount : null, x => x != null ? UserService.Domain.ValueObjects.Money.Create(x.Value, "USD") : null)
              .HasPrecision(10, 2);

        builder.Property(x => x.StorageMBPerFamily)
              .HasConversion(x => x.Megabytes, x => UserService.Domain.ValueObjects.StorageQuota.Create(x))
              .HasColumnName("StorageMBPerFamily");

        // Relationships
        builder.HasOne(x => x.FamilyPlan)
              .WithMany()
              .HasForeignKey(x => x.FamilyPlanId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubscriptionPlan)
              .WithMany()
              .HasForeignKey(x => x.SubscriptionPlanId)
              .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.FamilyPlanId);
        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => new { x.FamilyPlanId, x.IsActive });
    }
}
