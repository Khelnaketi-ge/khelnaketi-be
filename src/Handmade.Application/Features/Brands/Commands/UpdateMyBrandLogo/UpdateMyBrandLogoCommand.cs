using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;
using Handmade.Application.Features.Brands.Queries.GetMyBrand;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Commands.UpdateMyBrandLogo;

public sealed record UpdateMyBrandLogoCommand(int BrandId, IFormFile Logo) : IRequest<BrandOverviewDto>;

public sealed class UpdateMyBrandLogoCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage) : IRequestHandler<UpdateMyBrandLogoCommand, BrandOverviewDto>
{
    private const string BrandLogoFolder = "brand-logos";

    public async Task<BrandOverviewDto> Handle(UpdateMyBrandLogoCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var currentUserAccessLevel = await context.Users
            .Where(x => x.Id == currentUser.Id.Value)
            .Select(x => x.AccessLevel)
            .SingleOrDefaultAsync(cancellationToken);

        var isSuperAdmin = currentUserAccessLevel == AccessLevel.SuperAdmin;

        var brand = await context.Brands
            .Include(x => x.LogoImage)
            .Include(x => x.PhoneNumbers)
            .Include(x => x.EmailAddresses)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.BrandId, cancellationToken);

        if (brand is null || !isSuperAdmin && brand.OwnerUserId != currentUser.Id.Value)
        {
            throw new UnauthorizedException(UnauthorizedErrors.BrandOwnerRequired);
        }

        ImageUploadResult? uploadedLogo = null;
        var oldLogoObjectKey = brand.LogoImage?.ObjectKey;

        try
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

            var logoImage = new ImageAsset
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
            brand.LogoImageId = logoImage.Id;
            await context.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(oldLogoObjectKey))
            {
                await imageStorage.DeleteAsync(oldLogoObjectKey, CancellationToken.None);
            }

            return new BrandOverviewDto(
                brand.Id,
                brand.Name,
                brand.LegalName,
                brand.Status,
                brand.Created,
                logoImage.Id,
                uploadedLogo.PublicUrl,
                new BrandContactsDto(
                    brand.PhoneNumbers.Select(x => new BrandPhoneNumberDto(x.Id, x.PhoneNumber, x.Label, x.IsPrimary, x.IsActive)).ToList(),
                    brand.EmailAddresses.Select(x => new BrandEmailAddressDto(x.Id, x.Email, x.Label, x.IsPrimary, x.IsActive)).ToList(),
                    brand.Addresses.Select(x => new BrandAddressDto(
                        x.Id,
                        x.City,
                        x.AddressLine1,
                        x.AddressLine2,
                        x.PostalCode,
                        x.Latitude,
                        x.Longitude,
                        x.IsPrimary,
                        x.IsActive)).ToList()));
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
}
