using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandlerValidation : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        TranslationValidation.ValidateCategoryTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var category = await context.Categories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

                if (category is null)
                {
                    return true;
                }

                var ka = TranslationValidation.Georgian(
                    command.Translations,
                    translation => translation.LanguageCode);
                return !await context.Categories.AnyAsync(
                    x => x.Id != command.CategoryId
                        && x.ParentId == category.ParentId
                        && x.Translations.Any(t =>
                            t.LanguageCode == LanguageCodes.Georgian
                            && t.Name == ka.Name.Trim()),
                    cancellationToken);
            })
            .WithMessage("Category name already exists under this parent");
    }
}
