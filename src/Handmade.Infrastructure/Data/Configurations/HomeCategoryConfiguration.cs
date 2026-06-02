using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class HomeCategoryConfiguration : BaseAuditableEntityConfiguration<HomeCategory>
{
    public override void Configure(EntityTypeBuilder<HomeCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("HomeCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Order)
            .HasDefaultValue(0);

        builder.HasIndex(x => x.CategoryId)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => x.Order);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.HomeCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Image)
            .WithMany()
            .HasForeignKey(x => x.ImageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
