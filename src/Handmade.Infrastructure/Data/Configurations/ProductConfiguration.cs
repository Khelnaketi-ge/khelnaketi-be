using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ProductConfiguration : BaseAuditableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.Sku)
            .HasMaxLength(80);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.IsInStock)
            .HasDefaultValue(false);

        builder.Property(x => x.Status)
            .HasConversion<short>()
            .HasDefaultValue(ProductStatus.Draft);

        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.NormalizedName);

        builder.HasIndex(x => new { x.BrandId, x.Sku })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"Sku\" IS NOT NULL");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
