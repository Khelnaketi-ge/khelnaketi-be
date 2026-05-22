namespace Handmade.Application.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(ImageUploadRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed record ImageUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Folder = null);

public sealed record ImageUploadResult(
    Guid Id,
    string BucketName,
    string ObjectKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string? PublicUrl);
