using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandMemberConfiguration : BaseAuditableEntityConfiguration<BrandMember>
{
    public override void Configure(EntityTypeBuilder<BrandMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandMembers");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);

        builder.HasIndex(x => new { x.BrandId, x.UserId })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.BrandMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Members)
            .HasForeignKey(x => new { x.BrandId, x.RoleId })
            .HasPrincipalKey(x => new { x.BrandId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
