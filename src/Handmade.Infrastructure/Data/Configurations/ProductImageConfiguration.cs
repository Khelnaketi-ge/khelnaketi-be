using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ProductImageConfiguration : BaseAuditableEntityConfiguration<ProductImage>
{
    public override void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Order)
            .HasDefaultValue(0);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ImageId);

        builder.HasIndex(x => new { x.ProductId, x.ImageId })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.ProductId, x.IsPrimary })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"IsPrimary\" = true");

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Image)
            .WithMany()
            .HasForeignKey(x => x.ImageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
