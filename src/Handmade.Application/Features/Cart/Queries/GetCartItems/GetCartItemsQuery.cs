using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Cart.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Queries.GetCartItems;

public sealed record GetCartItemsQuery : IRequest<IReadOnlyCollection<CartItemDto>>;

public sealed class GetCartItemsQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage,
    ICurrentLanguage currentLanguage) : IRequestHandler<GetCartItemsQuery, IReadOnlyCollection<CartItemDto>>
{
    public async Task<IReadOnlyCollection<CartItemDto>> Handle(
        GetCartItemsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var languageCode = currentLanguage.Code;

        var items = await context.CartItems
            .AsNoTracking()
            .Where(x =>
                x.Cart.UserId == currentUser.Id.Value
                && x.Cart.Status == CartStatus.Active
                && x.Product.Status == ProductStatus.Active
                && x.Product.Brand.Status == BrandStatus.Active)
            .OrderByDescending(x => x.Created)
            .Select(x => new
            {
                x.Id,
                x.ProductId,
                x.Quantity,
                x.Product.Price,
                x.Product.IsInStock,
                Translation = x.Product.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Title, t.Slug })
                    .FirstOrDefault(),
                CategoryTranslation = x.Product.Category.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => new { t.Slug })
                    .FirstOrDefault(),
                PrimaryImageObjectKey = x.Product.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.Order)
                    .Select(image => image.Image.ObjectKey)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return items
            .Where(x => x.Translation is not null && x.CategoryTranslation is not null)
            .Select(x => new CartItemDto(
                x.Id,
                x.ProductId,
                x.Quantity,
                x.Translation!.Title,
                x.Translation.Slug,
                $"/{languageCode}/{x.CategoryTranslation!.Slug}/{x.Translation.Slug}",
                x.Price,
                x.IsInStock,
                x.PrimaryImageObjectKey is null ? null : imageStorage.GetPublicUrl(x.PrimaryImageObjectKey)))
            .ToList();
    }
}
