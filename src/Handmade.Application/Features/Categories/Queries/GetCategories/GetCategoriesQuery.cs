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
    IReadOnlyCollection<CategoryAttributeDto> Attributes);

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
            .Include(x => x.CategoryAttributes)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Options)
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var childrenByParentId = categories
            .GroupBy(x => x.ParentId)
            .ToDictionary(
                x => x.Key ?? 0,
                x => x.OrderBy(category => category.Name).ToList());

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
                    category.Name,
                    category.Description,
                    category.ParentId,
                    children.Count == 0,
                    children,
                    category.CategoryAttributes
                        .OrderBy(x => x.Order)
                        .ThenBy(x => x.ProductAttribute.Name)
                        .Select(attribute => new CategoryAttributeDto(
                            attribute.Id,
                            attribute.ProductAttributeId,
                            attribute.ProductAttribute.Name,
                            attribute.ProductAttribute.Type,
                            attribute.ProductAttribute.Unit,
                            attribute.IsRequired,
                            attribute.IsFilterable,
                            attribute.Order,
                            attribute.ProductAttribute.Options
                                .OrderBy(option => option.Order)
                                .ThenBy(option => option.Value)
                                .Select(option => new AttributeOptionDto(option.Id, option.Value, option.Order))
                                .ToList()))
                        .ToList());
            })
            .ToList();
    }
}
