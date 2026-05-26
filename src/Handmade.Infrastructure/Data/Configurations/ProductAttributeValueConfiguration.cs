using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ProductAttributeValueConfiguration : BaseAuditableEntityConfiguration<ProductAttributeValue>
{
    public override void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductAttributeValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductAttributeId);
        builder.HasIndex(x => x.AttributeOptionId);

        builder.HasIndex(x => new { x.ProductId, x.ProductAttributeId })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Product)
            .WithMany(x => x.AttributeValues)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductAttribute)
            .WithMany()
            .HasForeignKey(x => x.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AttributeOption)
            .WithMany()
            .HasForeignKey(x => x.AttributeOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
