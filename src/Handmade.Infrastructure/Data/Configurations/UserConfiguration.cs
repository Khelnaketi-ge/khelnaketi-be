using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class UserConfiguration : BaseAuditableEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(x => x.NormalizedPhoneNumber)
            .HasMaxLength(32);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512);

        builder.Property(x => x.TokenVersion)
            .HasDefaultValue(1);

        builder.Property(x => x.AccessFailedCount)
            .HasDefaultValue((short)0);

        builder.Property(x => x.EmailVerified)
            .HasDefaultValue(false);

        builder.Property(x => x.PhoneNumberVerified)
            .HasDefaultValue(false);

        builder.Property(x => x.IsBlocked)
            .HasDefaultValue(false);

        builder.Property(x => x.AccessLevel)
            .HasConversion<short>()
            .HasDefaultValue(AccessLevel.User);

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => x.NormalizedPhoneNumber)
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"NormalizedPhoneNumber\" IS NOT NULL");

        builder.HasIndex(x => x.AccessLevel);
        builder.HasIndex(x => x.IsBlocked);
    }
}
