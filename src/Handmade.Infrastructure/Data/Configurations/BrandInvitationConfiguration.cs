using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class BrandInvitationConfiguration : BaseAuditableEntityConfiguration<BrandInvitation>
{
    public override void Configure(EntityTypeBuilder<BrandInvitation> builder)
    {
        base.Configure(builder);

        builder.ToTable("BrandInvitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => x.InvitedByUserId);
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => new { x.BrandId, x.NormalizedEmail })
            .IsUnique()
            .HasFilter("\"Deleted\" = false AND \"AcceptedAt\" IS NULL AND \"RevokedAt\" IS NULL");

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => new { x.BrandId, x.RoleId })
            .HasPrincipalKey(x => new { x.BrandId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvitedByUser)
            .WithMany(x => x.SentBrandInvitations)
            .HasForeignKey(x => x.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
