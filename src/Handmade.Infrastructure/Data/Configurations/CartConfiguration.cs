using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class CartConfiguration : BaseAuditableEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        base.Configure(builder);

        builder.ToTable("Carts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<short>()
            .HasDefaultValue(CartStatus.Active);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new { x.UserId, x.Status })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"Status\" = 1");

        builder.HasOne(x => x.User)
            .WithMany(x => x.Carts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
