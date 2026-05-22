using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandEmailAddressConfiguration : BaseAuditableEntityConfiguration<BrandEmailAddress>
{
    public override void Configure(EntityTypeBuilder<BrandEmailAddress> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandEmailAddresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(80);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new { x.BrandId, x.NormalizedEmail })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.BrandId, x.IsPrimary })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"IsActive\" = true AND \"IsPrimary\" = true");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.EmailAddresses)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
