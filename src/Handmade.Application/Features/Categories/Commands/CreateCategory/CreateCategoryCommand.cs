using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    int? ParentId,
    IReadOnlyCollection<CategoryTranslationInput> Translations) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await context.Categories
                .Include(x => x.CategoryAttributes)
                .SingleOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
            {
                throw new ValidationException(nameof(request.ParentId), "Parent category was not found");
            }

            if (parent.CategoryAttributes.Count > 0)
            {
                throw new ValidationException(
                    nameof(request.ParentId),
                    "Child categories cannot be added under a category with linked attributes");
            }
        }

        var category = new Category
        {
            ParentId = request.ParentId
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var input in request.Translations)
        {
            var languageCode = LanguageCodes.Normalize(input.LanguageCode);
            var slug = await SlugGenerator.GenerateUniqueAsync(
                context.CategoryTranslations
                    .Where(x => x.LanguageCode == languageCode)
                    .Select(x => x.Slug),
                input.Name,
                200,
                cancellationToken);

            category.Translations.Add(new CategoryTranslation
            {
                CategoryId = category.Id,
                LanguageCode = languageCode,
                Name = input.Name.Trim(),
                Slug = slug
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        var displayTranslation = TranslationValidation.Georgian(
            category.Translations,
            translation => translation.LanguageCode);

        return new CategoryDto(
            category.Id,
            displayTranslation.Name,
            null,
            category.ParentId,
            true,
            [],
            category.Translations
                .Select(x => new CategoryTranslationDto(
                    x.LanguageCode,
                    x.Name,
                    x.Slug))
                .ToList(),
            []);
    }
}
