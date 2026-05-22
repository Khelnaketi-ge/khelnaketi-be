using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class RefreshTokenConfiguration : BaseAuditableEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedReason)
            .HasMaxLength(256);

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(512);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => x.RevokedAt);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
