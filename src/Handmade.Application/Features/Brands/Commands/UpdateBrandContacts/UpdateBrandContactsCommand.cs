using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;

public sealed record UpdateBrandContactsCommand(
    int BrandId,
    IReadOnlyCollection<BrandPhoneNumberInput> PhoneNumbers,
    IReadOnlyCollection<BrandEmailAddressInput> EmailAddresses,
    IReadOnlyCollection<BrandAddressInput> Addresses) : IRequest<BrandContactsDto>;

public sealed record BrandPhoneNumberInput(
    string PhoneNumber,
    string? Label,
    bool IsPrimary,
    bool IsActive = true);

public sealed record BrandEmailAddressInput(
    string Email,
    string? Label,
    bool IsPrimary,
    bool IsActive = true);

public sealed record BrandAddressInput(
    string City,
    string AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    bool IsPrimary,
    bool IsActive = true);

public sealed record BrandContactsDto(
    IReadOnlyCollection<BrandPhoneNumberDto> PhoneNumbers,
    IReadOnlyCollection<BrandEmailAddressDto> EmailAddresses,
    IReadOnlyCollection<BrandAddressDto> Addresses);

public sealed record BrandPhoneNumberDto(
    int Id,
    string PhoneNumber,
    string? Label,
    bool IsPrimary,
    bool IsActive);

public sealed record BrandEmailAddressDto(
    int Id,
    string Email,
    string? Label,
    bool IsPrimary,
    bool IsActive);

public sealed record BrandAddressDto(
    int Id,
    string City,
    string AddressLine1,
    string? AddressLine2,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    bool IsPrimary,
    bool IsActive);

public sealed class UpdateBrandContactsCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<UpdateBrandContactsCommand, BrandContactsDto>
{
    public async Task<BrandContactsDto> Handle(UpdateBrandContactsCommand request, CancellationToken cancellationToken)
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
            .Include(x => x.PhoneNumbers)
            .Include(x => x.EmailAddresses)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.BrandId, cancellationToken);

        if (brand is null || (!isSuperAdmin && brand.OwnerUserId != currentUser.Id.Value))
        {
            throw new UnauthorizedException(UnauthorizedErrors.BrandOwnerRequired);
        }

        context.BrandPhoneNumbers.RemoveRange(brand.PhoneNumbers);
        context.BrandEmailAddresses.RemoveRange(brand.EmailAddresses);
        context.BrandAddresses.RemoveRange(brand.Addresses);

        brand.PhoneNumbers = request.PhoneNumbers
            .Select(phoneNumber => new BrandPhoneNumber
            {
                BrandId = brand.Id,
                PhoneNumber = phoneNumber.PhoneNumber.Trim(),
                NormalizedPhoneNumber = NormalizePhoneNumber(phoneNumber.PhoneNumber),
                Label = NormalizeOptional(phoneNumber.Label),
                IsPrimary = phoneNumber.IsPrimary,
                IsActive = phoneNumber.IsActive
            })
            .ToList();

        brand.EmailAddresses = request.EmailAddresses
            .Select(emailAddress => new BrandEmailAddress
            {
                BrandId = brand.Id,
                Email = emailAddress.Email.Trim(),
                NormalizedEmail = NormalizeEmail(emailAddress.Email),
                Label = NormalizeOptional(emailAddress.Label),
                IsPrimary = emailAddress.IsPrimary,
                IsActive = emailAddress.IsActive
            })
            .ToList();

        brand.Addresses = request.Addresses
            .Select(address => new BrandAddress
            {
                BrandId = brand.Id,
                City = address.City.Trim(),
                AddressLine1 = address.AddressLine1.Trim(),
                AddressLine2 = NormalizeOptional(address.AddressLine2),
                PostalCode = NormalizeOptional(address.PostalCode),
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsPrimary = address.IsPrimary,
                IsActive = address.IsActive
            })
            .ToList();

        await context.SaveChangesAsync(cancellationToken);

        return ToDto(brand);
    }

    private static BrandContactsDto ToDto(Brand brand) =>
        new(
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
                .ToList());

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string NormalizePhoneNumber(string phoneNumber) =>
        new(phoneNumber.Where(char.IsDigit).ToArray());

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
