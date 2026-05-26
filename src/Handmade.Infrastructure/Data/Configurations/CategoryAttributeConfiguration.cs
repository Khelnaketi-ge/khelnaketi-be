using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class CategoryAttributeConfiguration : BaseAuditableEntityConfiguration<CategoryAttribute>
{
    public override void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("CategoryAttributes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.IsFilterable)
            .HasDefaultValue(false);

        builder.Property(x => x.Order)
            .HasDefaultValue(0);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.ProductAttributeId);

        builder.HasIndex(x => new { x.CategoryId, x.ProductAttributeId })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.CategoryAttributes)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductAttribute)
            .WithMany(x => x.CategoryAttributes)
            .HasForeignKey(x => x.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
