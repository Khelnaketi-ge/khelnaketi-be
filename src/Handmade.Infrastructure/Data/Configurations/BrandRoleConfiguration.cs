using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandRoleConfiguration : BaseAuditableEntityConfiguration<BrandRole>
{
    public override void Configure(EntityTypeBuilder<BrandRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsSystemRole)
            .HasDefaultValue(false);

        builder.Property(x => x.Permissions)
            .HasConversion<int>()
            .HasDefaultValue(Permissions.None);

        builder.HasAlternateKey(x => new { x.BrandId, x.Id });

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new { x.BrandId, x.NormalizedName })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
