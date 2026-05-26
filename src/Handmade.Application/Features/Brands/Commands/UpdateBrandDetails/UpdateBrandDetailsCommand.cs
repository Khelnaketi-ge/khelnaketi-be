using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;
using Handmade.Application.Features.Brands.Queries.GetMyBrand;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Commands.UpdateBrandDetails;

public sealed record UpdateBrandDetailsCommand(int BrandId, string Name, string? LegalName) : IRequest<BrandOverviewDto>;

public sealed class UpdateBrandDetailsCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage) : IRequestHandler<UpdateBrandDetailsCommand, BrandOverviewDto>
{
    public async Task<BrandOverviewDto> Handle(UpdateBrandDetailsCommand request, CancellationToken cancellationToken)
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

        if (brand is null || (!isSuperAdmin && brand.OwnerUserId != currentUser.Id.Value))
        {
            throw new UnauthorizedException(UnauthorizedErrors.BrandOwnerRequired);
        }

        brand.Name = request.Name.Trim();
        brand.LegalName = string.IsNullOrWhiteSpace(request.LegalName) ? null : request.LegalName.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return new BrandOverviewDto(
            brand.Id,
            brand.Name,
            brand.LegalName,
            brand.Status,
            brand.Created,
            brand.LogoImageId,
            brand.LogoImage is null ? null : imageStorage.GetPublicUrl(brand.LogoImage.ObjectKey),
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
}
