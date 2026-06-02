using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ProductAttributeConfiguration : BaseAuditableEntityConfiguration<ProductAttribute>
{
    public override void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductAttributes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(32);

        builder.Property(x => x.IsDisabled)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.IsDisabled);

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");
    }
}
