using Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;
using Handmade.Application.Features.Brands.Queries.GetMyBrand;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Queries.GetBrands;

public sealed record GetBrandsQuery : IRequest<IReadOnlyCollection<BrandOverviewDto>>;

public sealed class GetBrandsQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage) : IRequestHandler<GetBrandsQuery, IReadOnlyCollection<BrandOverviewDto>>
{
    public async Task<IReadOnlyCollection<BrandOverviewDto>> Handle(
        GetBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var brands = await context.Brands
            .AsNoTracking()
            .Include(x => x.LogoImage)
            .Include(x => x.PhoneNumbers)
            .Include(x => x.EmailAddresses)
            .Include(x => x.Addresses)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return brands
            .Select(brand => new BrandOverviewDto(
                brand.Id,
                brand.Name,
                brand.LegalName,
                brand.Status,
                brand.Created,
                brand.LogoImageId,
                brand.LogoImage == null ? null : imageStorage.GetPublicUrl(brand.LogoImage.ObjectKey),
                brand.Slug,
                brand.Description,
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
                        .ToList())))
            .ToList();
    }
}
