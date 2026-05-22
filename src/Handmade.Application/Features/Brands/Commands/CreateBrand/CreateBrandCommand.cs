using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Brands.Commands.CreateBrand.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;

namespace Handmade.Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    int OwnerUserId,
    string? LegalName,
    UploadFileInput? Logo) : IRequest<CreateBrandDto>;

public sealed record UploadFileInput(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes);

public sealed class CreateBrandCommandHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    ICurrentUser currentUser) : IRequestHandler<CreateBrandCommand, CreateBrandDto>
{
    private const string BrandLogoFolder = "brand-logos";

    public async Task<CreateBrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        ImageUploadResult? uploadedLogo = null;

        try
        {
            ImageAsset? logoImage = null;

            if (request.Logo is not null)
            {
                uploadedLogo = await imageStorage.UploadAsync(
                    new ImageUploadRequest(
                        request.Logo.Content,
                        request.Logo.FileName,
                        request.Logo.ContentType,
                        request.Logo.SizeBytes,
                        BrandLogoFolder),
                    cancellationToken);

                logoImage = new ImageAsset
                {
                    Id = uploadedLogo.Id,
                    BucketName = uploadedLogo.BucketName,
                    ObjectKey = uploadedLogo.ObjectKey,
                    OriginalFileName = uploadedLogo.OriginalFileName,
                    ContentType = uploadedLogo.ContentType,
                    SizeBytes = uploadedLogo.SizeBytes,
                    UploadedByUserId = currentUser.Id.Value
                };

                context.ImageAssets.Add(logoImage);
            }

            var brand = new Brand
            {
                Name = request.Name.Trim(),
                NormalizedName = NormalizeName(request.Name),
                LegalName = NormalizeOptional(request.LegalName),
                LogoImageId = logoImage?.Id,
                OwnerUserId = request.OwnerUserId
            };

            context.Brands.Add(brand);

            await context.SaveChangesAsync(cancellationToken);

            return new CreateBrandDto(
                brand.Id,
                logoImage?.Id,
                uploadedLogo?.PublicUrl);
        }
        catch
        {
            if (uploadedLogo is not null)
            {
                await imageStorage.DeleteAsync(uploadedLogo.ObjectKey, CancellationToken.None);
            }

            throw;
        }
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
