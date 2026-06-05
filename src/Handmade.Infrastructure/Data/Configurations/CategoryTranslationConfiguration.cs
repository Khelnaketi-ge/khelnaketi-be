using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class CategoryTranslationConfiguration : BaseAuditableEntityConfiguration<CategoryTranslation>
{
    public override void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        base.Configure(builder);

        builder.ToTable("CategoryTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => new { x.LanguageCode, x.Slug })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.CategoryId, x.LanguageCode })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
