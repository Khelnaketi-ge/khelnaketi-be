using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Queries.GetHomeCategories;

public sealed record GetHomeCategoriesQuery(string LanguageCode = LanguageCodes.Georgian)
    : IRequest<IReadOnlyCollection<HomeCategoryDto>>;

public sealed record HomeCategoryDto(
    int Id,
    int CategoryId,
    string Name,
    string Slug,
    string? Description,
    int? ParentId,
    bool IsLeaf,
    int Order,
    Guid? ImageId,
    string? ImageUrl);

public sealed class GetHomeCategoriesQueryHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage)
    : IRequestHandler<GetHomeCategoriesQuery, IReadOnlyCollection<HomeCategoryDto>>
{
    public async Task<IReadOnlyCollection<HomeCategoryDto>> Handle(
        GetHomeCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var languageCode = LanguageCodes.IsSupported(request.LanguageCode)
            ? LanguageCodes.Normalize(request.LanguageCode)
            : LanguageCodes.Georgian;

        var homeCategories = await context.HomeCategories
            .AsNoTracking()
            .Include(x => x.Category)
                .ThenInclude(x => x.Children)
            .Include(x => x.Category)
                .ThenInclude(x => x.Translations)
            .Include(x => x.Image)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        return homeCategories.Select(x =>
            {
                var translation = HomeCategoryDtoMapper.GetTranslation(x.Category, languageCode);

                return new HomeCategoryDto(
                x.Id,
                x.CategoryId,
                translation.Name,
                translation.Slug,
                null,
                x.Category.ParentId,
                !x.Category.Children.Any(),
                x.Order,
                x.ImageId,
                x.Image == null ? null : imageStorage.GetPublicUrl(x.Image.ObjectKey));
            })
            .ToList();
    }
}

public static class HomeCategoryDtoMapper
{
    public static Domain.Entities.CategoryTranslation GetTranslation(
        Domain.Entities.Category category,
        string languageCode) =>
        category.Translations.FirstOrDefault(x => x.LanguageCode == languageCode)
        ?? category.Translations.First(x => x.LanguageCode == LanguageCodes.Georgian);
}
