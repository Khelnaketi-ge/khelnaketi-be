using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class UserSessionConfiguration : BaseAuditableEntityConfiguration<UserSession>
{
    public override void Configure(EntityTypeBuilder<UserSession> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.Property(x => x.RevokedReason)
            .HasMaxLength(256);

        builder.Ignore(x => x.IsRevoked);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.LastUsedAt);
        builder.HasIndex(x => x.RevokedAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
