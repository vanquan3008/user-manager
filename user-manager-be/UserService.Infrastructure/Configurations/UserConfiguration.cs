using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Username, username =>
        {
            username.Property(u => u.Value)
                    .HasColumnName("Username")
                    .IsRequired()
                    .HasMaxLength(30);
            
            username.HasIndex(u => u.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Username");
        });

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(e => e.Value)
                 .HasColumnName("Email")
                 .IsRequired()
                 .HasMaxLength(256);
            
            email.HasIndex(e => e.Value)
                 .IsUnique()
                 .HasDatabaseName("IX_Users_Email");
        });

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(100);
            name.Property(n => n.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(100);
            name.Property(n => n.DisplayName).HasColumnName("DisplayName").IsRequired().HasMaxLength(200);
        });

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
            address.Property(a => a.StreetAddress).HasColumnName("Address").HasMaxLength(500);
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
        });

        builder.Property(x => x.PhoneNumber)
              .HasMaxLength(20);

        builder.Property(x => x.AvatarUrl)
              .HasMaxLength(500);

        builder.Property(x => x.CoverImageUrl)
              .HasMaxLength(500);

        builder.Property(x => x.Bio)
              .HasMaxLength(1000);

        builder.Property(x => x.Website)
              .HasMaxLength(500);

        builder.Property(x => x.PreferredLanguage)
              .HasMaxLength(10);

        builder.Property(x => x.TimeZone)
              .HasMaxLength(50);

        builder.Property(x => x.Gender)
              .HasConversion<string?>()
              .HasMaxLength(50);
    }
}
