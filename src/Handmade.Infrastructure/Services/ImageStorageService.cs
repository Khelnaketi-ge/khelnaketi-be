using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using ApplicationException = Handmade.Application.Common.Exceptions.ApplicationException;

namespace Handmade.Infrastructure.Services;

public sealed class ImageStorageService(
    SupabaseStorage storage,
    HttpClient httpClient,
    ILogger<ImageStorageService> logger) : IImageStorageService
{
    public async Task<ImageUploadResult> UploadAsync(ImageUploadRequest request, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        ValidateUpload(request);

        var imageId = Guid.NewGuid();
        var contentType = NormalizeContentType(request.ContentType);
        var objectKey = BuildObjectKey(request.Folder, imageId, GetExtension(contentType));

        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length is <= 0 || buffer.Length > storage.MaxFileSizeBytes)
        {
            throw new DomainException(ImageErrors.InvalidSize);
        }

        buffer.Position = 0;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildObjectUri(objectKey));
        httpRequest.Content = new StreamContent(buffer);

        AddSupabaseHeaders(httpRequest);
        httpRequest.Headers.TryAddWithoutValidation("cache-control", storage.CacheControlSeconds.ToString());
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        httpRequest.Content.Headers.ContentLength = buffer.Length;

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        
        if (response.IsSuccessStatusCode)
            return new ImageUploadResult(
                imageId,
                storage.BucketName,
                objectKey,
                NormalizeFileName(request.FileName),
                contentType,
                buffer.Length,
                BuildPublicUrl(objectKey));
        
        var error = await ReadSupabaseErrorAsync(response, cancellationToken);
        logger.LogWarning(
            "Supabase image upload failed with status {StatusCode}. Error: {Error}",
            (int)response.StatusCode,
            error ?? "Unknown error");

        throw new ApplicationException("Image upload failed.");
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            EnsureConfigured();

            objectKey = NormalizeObjectKey(objectKey);
            
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, BuildBucketObjectUri());
            httpRequest.Content = JsonContent.Create(new DeleteObjectsRequest([objectKey]));

            AddSupabaseHeaders(httpRequest);

            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            logger.LogWarning(
                "Supabase image delete failed for object key {ObjectKey} with status {StatusCode}.",
                objectKey,
                (int)response.StatusCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Supabase image delete failed for object key {ObjectKey}.", objectKey);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(storage.StorageBaseUrl)
            || string.IsNullOrWhiteSpace(storage.BucketName)
            || string.IsNullOrWhiteSpace(storage.ApiKey))
        {
            throw new ApplicationException("Supabase storage is not configured.");
        }
    }

    private void ValidateUpload(ImageUploadRequest request)
    {
        if (request.Content is null || !request.Content.CanRead)
        {
            throw new DomainException(ImageErrors.ContentRequired);
        }

        if (request.SizeBytes is <= 0 || request.SizeBytes > storage.MaxFileSizeBytes)
        {
            throw new DomainException(ImageErrors.InvalidSize);
        }

        var contentType = NormalizeContentType(request.ContentType);
        var allowedContentTypes = storage.AllowedMimeTypes.Select(NormalizeContentType).ToHashSet(StringComparer.Ordinal);

        if (!allowedContentTypes.Contains(contentType))
        {
            throw new DomainException(ImageErrors.ContentTypeNotAllowed);
        }
    }

    private void AddSupabaseHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("apikey", storage.ApiKey);

        var authorizationToken = string.IsNullOrWhiteSpace(storage.AuthorizationToken)
            ? GetLegacyJwtApiKey()
            : storage.AuthorizationToken;

        if (!string.IsNullOrWhiteSpace(authorizationToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);
        }
    }

    private string? GetLegacyJwtApiKey() =>
        storage.ApiKey.StartsWith("eyJ", StringComparison.Ordinal)
            ? storage.ApiKey
            : null;

    private Uri BuildObjectUri(string objectKey)
    {
        var objectPath = $"object/{Uri.EscapeDataString(storage.BucketName.Trim('/'))}/{EscapeObjectKey(objectKey)}";
        return new Uri(new Uri(storage.StorageBaseUrl.TrimEnd('/') + "/"), objectPath);
    }

    private Uri BuildBucketObjectUri()
    {
        var objectPath = $"object/{Uri.EscapeDataString(storage.BucketName.Trim('/'))}";
        return new Uri(new Uri(storage.StorageBaseUrl.TrimEnd('/') + "/"), objectPath);
    }

    private string? BuildPublicUrl(string objectKey)
    {
        return string.IsNullOrWhiteSpace(storage.PublicBaseUrl) 
            ? null 
            : $"{storage.PublicBaseUrl.TrimEnd('/')}/" +
              $"{Uri.EscapeDataString(storage.BucketName.Trim('/'))}" +
              $"/{EscapeObjectKey(objectKey)}";
    }

    private string BuildObjectKey(string? folder, Guid imageId, string extension)
    {
        var normalizedFolder = NormalizeFolder(folder ?? storage.DefaultFolder);
        return $"{normalizedFolder}/{imageId:N}{extension}";
    }

    private static string NormalizeContentType(string contentType)
    {
        return string.IsNullOrWhiteSpace(contentType) 
            ? throw new DomainException(ImageErrors.ContentTypeRequired) 
            : contentType.Trim().ToLowerInvariant();
    }

    private static string GetExtension(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new DomainException(ImageErrors.ContentTypeNotAllowed)
    };

    private static string NormalizeFolder(string folder)
    {
        folder = folder.Trim().Replace('\\', '/').Trim('/');

        if (string.IsNullOrWhiteSpace(folder))
        {
            return "images";
        }

        var segments = folder.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment => segment.Any(character =>
            !char.IsLetterOrDigit(character) && character is not '-' and not '_')) 
                ? throw new DomainException(ImageErrors.InvalidFolder) 
                : string.Join('/', segments);
    }

    private static string NormalizeObjectKey(string objectKey)
    {
        objectKey = objectKey.Trim().Replace('\\', '/').Trim('/');

        if (string.IsNullOrWhiteSpace(objectKey) || objectKey.Contains("..", StringComparison.Ordinal))
        {
            throw new DomainException(ImageErrors.InvalidObjectKey);
        }

        return objectKey;
    }

    private static string EscapeObjectKey(string objectKey) =>
        string.Join('/', objectKey.Split('/').Select(Uri.EscapeDataString));

    private static string NormalizeFileName(string fileName) =>
        string.IsNullOrWhiteSpace(fileName) ? "image" : Path.GetFileName(fileName.Trim());

    private static async Task<string?> ReadSupabaseErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            var error = JsonSerializer.Deserialize<SupabaseErrorResponse>(content);
            return error?.Message ?? error?.Error ?? content;
        }
        catch (JsonException)
        {
            return content;
        }
    }

    private sealed record DeleteObjectsRequest([property: JsonPropertyName("prefixes")] string[] Prefixes);

    private sealed record SupabaseErrorResponse(
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("message")] string? Message);
}
