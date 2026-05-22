namespace Handmade.Application.Features.Brands.Commands.CreateBrand.Models;

public sealed record CreateBrandDto(
    int Id,
    Guid? LogoImageId,
    string? LogoImageUrl);
