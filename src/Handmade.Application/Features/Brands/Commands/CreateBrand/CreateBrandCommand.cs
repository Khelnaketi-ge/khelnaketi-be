using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Brands.Commands.CreateBrand.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Handmade.Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    int OwnerUserId,
    string? LegalName,
    IFormFile? Logo) : IRequest<CreateBrandDto>;

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
                await using var logoStream = request.Logo.OpenReadStream();

                uploadedLogo = await imageStorage.UploadAsync(
                    new ImageUploadRequest(
                        logoStream,
                        request.Logo.FileName,
                        request.Logo.ContentType,
                        request.Logo.Length,
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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
