namespace Handmade.Infrastructure.Options;

public sealed class SupabaseStorage
{
    public string StorageBaseUrl { get; set; } = "https://ahtvudixigzcqjbuipvo.storage.supabase.co/storage/v1";
    public string? PublicBaseUrl { get; set; } = "https://ahtvudixigzcqjbuipvo.storage.supabase.co/storage/v1/object/public";
    public string BucketName { get; set; } = "public-images";
    public string ApiKey { get; set; } = string.Empty;
    public string AuthorizationToken { get; set; } = string.Empty;
    public string DefaultFolder { get; set; } = "images";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int CacheControlSeconds { get; set; } = 3600;
    public string[] AllowedMimeTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}
