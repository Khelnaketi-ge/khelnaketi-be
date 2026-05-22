using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class VerificationCodeConfiguration : BaseAuditableEntityConfiguration<VerificationCode>
{
    public override void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        base.Configure(builder);

        builder.ToTable("VerificationCodes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Purpose)
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Destination)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.FailedAttempts)
            .HasDefaultValue((short)0);

        builder.HasIndex(x => new { x.UserId, x.Purpose, x.UsedAt });
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne(x => x.User)
            .WithMany(x => x.VerificationCodes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
