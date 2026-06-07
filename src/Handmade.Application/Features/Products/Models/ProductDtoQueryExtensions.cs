using Handmade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Models;

public static class ProductDtoQueryExtensions
{
    public static IQueryable<Product> IncludeProductDtoGraph(this IQueryable<Product> query)
    {
        return query
            .Include(x => x.Brand)
            .Include(x => x.Category)
                .ThenInclude(x => x.Translations)
            .Include(x => x.Translations)
            .Include(x => x.Images)
                .ThenInclude(x => x.Image)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Translations)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
                    .ThenInclude(x => x!.Translations)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption);
    }
}
