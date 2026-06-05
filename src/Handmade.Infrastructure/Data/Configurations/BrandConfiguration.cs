using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandConfiguration : BaseAuditableEntityConfiguration<Brand>
{
    public override void Configure(EntityTypeBuilder<Brand> builder)
    {
        base.Configure(builder);

        builder.ToTable("Brands");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.LegalName)
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasConversion<short>()
            .HasDefaultValue(BrandStatus.Active);

        builder.HasIndex(x => x.OwnerUserId);
        builder.HasIndex(x => x.LogoImageId);
        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.OwnerUser)
            .WithMany(x => x.OwnedBrands)
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LogoImage)
            .WithMany()
            .HasForeignKey(x => x.LogoImageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
