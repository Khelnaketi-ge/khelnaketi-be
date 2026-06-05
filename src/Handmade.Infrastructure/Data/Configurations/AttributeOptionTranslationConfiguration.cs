using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class AttributeOptionTranslationConfiguration : BaseAuditableEntityConfiguration<AttributeOptionTranslation>
{
    public override void Configure(EntityTypeBuilder<AttributeOptionTranslation> builder)
    {
        base.Configure(builder);

        builder.ToTable("AttributeOptionTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LanguageCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => new { x.LanguageCode, x.Slug })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.AttributeOptionId, x.LanguageCode })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.AttributeOption)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.AttributeOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
