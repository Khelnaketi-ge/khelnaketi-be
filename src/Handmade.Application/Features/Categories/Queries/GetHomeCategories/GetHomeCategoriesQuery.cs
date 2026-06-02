using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Queries.GetHomeCategories;

public sealed record GetHomeCategoriesQuery : IRequest<IReadOnlyCollection<HomeCategoryDto>>;

public sealed record HomeCategoryDto(
    int Id,
    int CategoryId,
    string Name,
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
        var homeCategories = await context.HomeCategories
            .AsNoTracking()
            .Include(x => x.Category)
                .ThenInclude(x => x.Children)
            .Include(x => x.Image)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Category.Name)
            .ToListAsync(cancellationToken);

        return homeCategories.Select(x => new HomeCategoryDto(
                x.Id,
                x.CategoryId,
                x.Category.Name,
                x.Category.Description,
                x.Category.ParentId,
                !x.Category.Children.Any(),
                x.Order,
                x.ImageId,
                x.Image == null ? null : imageStorage.GetPublicUrl(x.Image.ObjectKey)))
            .ToList();
    }
}
