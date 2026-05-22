using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class UserExternalLoginConfiguration : BaseAuditableEntityConfiguration<UserExternalLogin>
{
    public override void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserExternalLogins");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Provider)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ProviderEmail)
            .HasMaxLength(320);

        builder.Property(x => x.ProviderDisplayName)
            .HasMaxLength(200);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
