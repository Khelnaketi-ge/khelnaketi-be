using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Queries.GetMyBrand;

public sealed record GetMyBrandQuery : IRequest<BrandOverviewDto?>;

public sealed record BrandOverviewDto(
    int Id,
    string Name,
    string? LegalName,
    BrandStatus Status,
    DateTimeOffset Created,
    Guid? LogoImageId,
    string? LogoImageUrl,
    BrandContactsDto Contacts);

public sealed class GetMyBrandQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage) : IRequestHandler<GetMyBrandQuery, BrandOverviewDto?>
{
    public async Task<BrandOverviewDto?> Handle(GetMyBrandQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var brand = await context.Brands
            .AsNoTracking()
            .Include(x => x.LogoImage)
            .Include(x => x.PhoneNumbers)
            .Include(x => x.EmailAddresses)
            .Include(x => x.Addresses)
            .Where(x => x.OwnerUserId == currentUser.Id.Value)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (brand is null)
        {
            return null;
        }

        return new BrandOverviewDto(
            brand.Id,
            brand.Name,
            brand.LegalName,
            brand.Status,
            brand.Created,
            brand.LogoImageId,
            brand.LogoImage is null ? null : imageStorage.GetPublicUrl(brand.LogoImage.ObjectKey),
            new BrandContactsDto(
                brand.PhoneNumbers
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Id)
                    .Select(x => new BrandPhoneNumberDto(x.Id, x.PhoneNumber, x.Label, x.IsPrimary, x.IsActive))
                    .ToList(),
                brand.EmailAddresses
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Id)
                    .Select(x => new BrandEmailAddressDto(x.Id, x.Email, x.Label, x.IsPrimary, x.IsActive))
                    .ToList(),
                brand.Addresses
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Id)
                    .Select(x => new BrandAddressDto(
                        x.Id,
                        x.City,
                        x.AddressLine1,
                        x.AddressLine2,
                        x.PostalCode,
                        x.Latitude,
                        x.Longitude,
                        x.IsPrimary,
                        x.IsActive))
                    .ToList()));
    }
}
