using Handmade.Application.Common.Localization;
using Handmade.Application.Features.Seo.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Seo.Queries.GetBrandBySlug;

public sealed record GetBrandBySlugQuery(string LanguageCode, string Slug) : IRequest<BrandSeoDto?>;

public sealed class GetBrandBySlugQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage) : IRequestHandler<GetBrandBySlugQuery, BrandSeoDto?>
{
    public async Task<BrandSeoDto?> Handle(GetBrandBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim();

        var languageCode = LanguageCodes.Normalize(request.LanguageCode);

        var item = await context.Brands
            .AsNoTracking()
            .Where(x => x.Slug == slug && x.Status == BrandStatus.Active)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                UpdatedAt = x.Updated ?? x.Created,
                LogoObjectKey = x.LogoImage == null ? null : x.LogoImage.ObjectKey
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? null
            : new BrandSeoDto(
                item.Id,
                item.Name,
                item.Slug,
                $"/{languageCode}/brands/{item.Slug}",
                item.Description,
                item.LogoObjectKey is null ? null : imageStorage.GetPublicUrl(item.LogoObjectKey),
                item.UpdatedAt);
    }
}
