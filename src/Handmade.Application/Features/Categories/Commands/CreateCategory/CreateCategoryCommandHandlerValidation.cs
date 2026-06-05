using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandlerValidation : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandHandlerValidation(IApplicationDbContext context)
    {
        TranslationValidation.ValidateCategoryTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Translations)
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var ka = TranslationValidation.Georgian(
                    command.Translations,
                    translation => translation.LanguageCode);
                return !await context.Categories.AnyAsync(
                    x => x.ParentId == command.ParentId
                        && x.Translations.Any(t =>
                            t.LanguageCode == LanguageCodes.Georgian
                            && t.Name == ka.Name.Trim()),
                    cancellationToken);
            })
            .WithMessage("Category name already exists under this parent");
    }
}
