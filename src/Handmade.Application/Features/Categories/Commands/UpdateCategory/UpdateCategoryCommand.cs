using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    int CategoryId,
    IReadOnlyCollection<CategoryTranslationInput> Translations) : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(x => x.Children)
            .Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        category.Translations.Clear();
        foreach (var input in request.Translations)
        {
            var languageCode = LanguageCodes.Normalize(input.LanguageCode);
            var slug = await SlugGenerator.GenerateUniqueAsync(
                context.CategoryTranslations
                    .Where(x => x.LanguageCode == languageCode && x.CategoryId != category.Id)
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
            category.Children.Count == 0,
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
