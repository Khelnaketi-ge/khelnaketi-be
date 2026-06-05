using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.UpdateAttribute;

public sealed record UpdateAttributeCommand(
    int AttributeId,
    string? Unit,
    IReadOnlyCollection<AttributeTranslationInput> Translations) : IRequest<AttributeDto>;

public sealed class UpdateAttributeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAttributeCommand, AttributeDto>
{
    public async Task<AttributeDto> Handle(
        UpdateAttributeCommand request,
        CancellationToken cancellationToken)
    {
        var attribute = await context.ProductAttributes
            .Include(x => x.Options)
            .Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);

        if (attribute is null)
        {
            throw new ValidationException(nameof(request.AttributeId), "Attribute was not found");
        }

        attribute.Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim();

        foreach (var translation in attribute.Translations.ToList())
        {
            context.ProductAttributeTranslations.Remove(translation);
        }

        attribute.Translations.Clear();
        foreach (var input in request.Translations)
        {
            attribute.Translations.Add(new ProductAttributeTranslation
            {
                ProductAttributeId = attribute.Id,
                LanguageCode = LanguageCodes.Normalize(input.LanguageCode),
                Name = input.Name.Trim(),
                Slug = SlugGenerator.GenerateForEntity(input.Name, "a", attribute.Id, 200)
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return AttributeMappings.ToDto(attribute);
    }
}
