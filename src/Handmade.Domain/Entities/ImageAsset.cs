using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class ImageAsset : BaseAuditableEntity<Guid>
{
    public required string BucketName { get; set; }
    public required string ObjectKey { get; set; }

    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }

    public long SizeBytes { get; set; }

    public int UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;

    public int? Width { get; set; }
    public int? Height { get; set; }
}