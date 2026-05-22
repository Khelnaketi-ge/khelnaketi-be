using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandAddressConfiguration : BaseAuditableEntityConfiguration<BrandAddress>
{
    public override void Configure(EntityTypeBuilder<BrandAddress> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandAddresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.City)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.AddressLine1)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(250);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(32);

        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new { x.BrandId, x.IsPrimary })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
