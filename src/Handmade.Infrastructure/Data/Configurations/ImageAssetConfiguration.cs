using Handmade.Domain.Entities;
using Handmade.Infrastructure.Data.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations;

internal class ImageAssetConfiguration : BaseAuditableEntityConfiguration<ImageAsset>
{
    public override void Configure(EntityTypeBuilder<ImageAsset> builder)
    {
        base.Configure(builder);

        builder.ToTable("ImageAssets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.BucketName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ObjectKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.HasIndex(x => x.UploadedByUserId);
        builder.HasIndex(x => x.ContentType);

        builder.HasIndex(x => new { x.BucketName, x.ObjectKey })
            .IsUnique()
            .HasFilter("\"Deleted\" = false");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedImages)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
