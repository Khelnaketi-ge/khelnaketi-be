using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyCollection<CategoryDto>>;

public sealed record CategoryDto(
    int Id,
    string Name,
    string? Description,
    int? ParentId,
    bool IsLeaf,
    IReadOnlyCollection<CategoryDto> Children,
    IReadOnlyCollection<CategoryTranslationDto> Translations,
    IReadOnlyCollection<CategoryAttributeDto> Attributes);

public sealed record CategoryTranslationDto(
    string LanguageCode,
    string Name,
    string Slug);

public sealed record CategoryAttributeDto(
    int Id,
    int AttributeId,
    string Name,
    Domain.Enums.AttributeType Type,
    string? Unit,
    bool IsRequired,
    bool IsFilterable,
    int Order,
    IReadOnlyCollection<AttributeOptionDto> Options);

public sealed class GetCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryDto>>
{
    public async Task<IReadOnlyCollection<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Include(x => x.Translations)
            .Include(x => x.CategoryAttributes)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Translations)
            .Include(x => x.CategoryAttributes)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Options)
                        .ThenInclude(x => x.Translations)
            .OrderBy(x => x.ParentId)
            .ToListAsync(cancellationToken);

        var childrenByParentId = categories
            .GroupBy(x => x.ParentId)
            .ToDictionary(
                x => x.Key ?? 0,
                x => x.OrderBy(GetCategoryName).ToList());

        return BuildTree(null, childrenByParentId);
    }

    private static IReadOnlyCollection<CategoryDto> BuildTree(
        int? parentId,
        IReadOnlyDictionary<int, List<Domain.Entities.Category>> childrenByParentId)
    {
        if (!childrenByParentId.TryGetValue(parentId ?? 0, out var categories))
        {
            return [];
        }

        return categories
            .Select(category =>
            {
                var children = BuildTree(category.Id, childrenByParentId);

                return new CategoryDto(
                    category.Id,
                    GetCategoryName(category),
                    null,
                    category.ParentId,
                    children.Count == 0,
                    children,
                    category.Translations
                        .OrderBy(x => x.LanguageCode)
                        .Select(x => new CategoryTranslationDto(
                            x.LanguageCode,
                            x.Name,
                            x.Slug))
                        .ToList(),
                    category.CategoryAttributes
                        .OrderBy(x => x.Order)
                        .ThenBy(x => AttributeMappings.GetAttributeName(x.ProductAttribute))
                        .Select(attribute => new CategoryAttributeDto(
                            attribute.Id,
                            attribute.ProductAttributeId,
                            AttributeMappings.GetAttributeName(attribute.ProductAttribute),
                            attribute.ProductAttribute.Type,
                            attribute.ProductAttribute.Unit,
                            attribute.IsRequired,
                            attribute.IsFilterable,
                            attribute.Order,
                            attribute.ProductAttribute.Options
                                .OrderBy(option => option.Order)
                                .ThenBy(AttributeMappings.GetOptionValue)
                                .Select(option => new AttributeOptionDto(
                                    option.Id,
                                    AttributeMappings.GetOptionValue(option),
                                    option.Order,
                                    option.Translations
                                        .OrderBy(translation => translation.LanguageCode)
                                        .Select(translation => new AttributeOptionTranslationDto(
                                            translation.LanguageCode,
                                            translation.Value,
                                            translation.Slug))
                                        .ToList()))
                                .ToList()))
                        .ToList());
            })
            .ToList();
    }

    private static string GetCategoryName(Domain.Entities.Category category) =>
        category.Translations
            .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)?.Name
        ?? category.Translations.FirstOrDefault()?.Name
        ?? $"Category {category.Id}";
}
