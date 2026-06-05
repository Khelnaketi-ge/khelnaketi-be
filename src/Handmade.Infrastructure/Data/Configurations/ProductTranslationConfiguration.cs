using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ProductTranslationConfiguration : BaseAuditableEntityConfiguration<ProductTranslation>
{
    public override void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
        builder.Property(x => x.Description).HasMaxLength(4000);

        builder.HasIndex(x => new { x.ProductId, x.LanguageCode })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.LanguageCode, x.Slug })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
