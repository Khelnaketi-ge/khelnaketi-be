using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandPhoneNumberConfiguration : BaseAuditableEntityConfiguration<BrandPhoneNumber>
{
    public override void Configure(EntityTypeBuilder<BrandPhoneNumber> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandPhoneNumbers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.NormalizedPhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(80);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new { x.BrandId, x.NormalizedPhoneNumber })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.BrandId, x.IsPrimary })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.PhoneNumbers)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
