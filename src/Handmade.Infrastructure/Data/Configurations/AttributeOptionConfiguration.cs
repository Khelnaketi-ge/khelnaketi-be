using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class AttributeOptionConfiguration : BaseAuditableEntityConfiguration<AttributeOption>
{
    public override void Configure(EntityTypeBuilder<AttributeOption> builder)
    {
        base.Configure(builder);

        builder.ToTable("AttributeOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.NormalizedValue)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Order)
            .HasDefaultValue(0);

        builder.HasIndex(x => x.CategoryAttributeId);

        builder.HasIndex(x => new { x.CategoryAttributeId, x.NormalizedValue })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.CategoryAttribute)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.CategoryAttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
