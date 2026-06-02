using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Categories.Queries.GetHomeCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateHomeCategory;

public sealed record UpdateHomeCategoryCommand(
    int CategoryId,
    bool IsVisible,
    int Order) : IRequest<HomeCategoryDto?>;

public sealed class UpdateHomeCategoryCommandHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage)
    : IRequestHandler<UpdateHomeCategoryCommand, HomeCategoryDto?>
{
    public async Task<HomeCategoryDto?> Handle(
        UpdateHomeCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(x => x.Children)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        var homeCategory = await context.HomeCategories
            .Include(x => x.Image)
            .SingleOrDefaultAsync(x => x.CategoryId == request.CategoryId, cancellationToken);

        if (!request.IsVisible)
        {
            if (homeCategory is not null)
            {
                context.HomeCategories.Remove(homeCategory);
                await context.SaveChangesAsync(cancellationToken);
            }

            return null;
        }

        homeCategory ??= new HomeCategory { CategoryId = request.CategoryId };
        homeCategory.Category = category;
        homeCategory.Order = request.Order;

        if (homeCategory.Id == 0)
        {
            context.HomeCategories.Add(homeCategory);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new HomeCategoryDto(
            homeCategory.Id,
            category.Id,
            category.Name,
            category.Description,
            category.ParentId,
            category.Children.Count == 0,
            homeCategory.Order,
            homeCategory.ImageId,
            homeCategory.Image is null ? null : imageStorage.GetPublicUrl(homeCategory.Image.ObjectKey));
    }
}
