using FluentValidation;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Commands.CreateAttribute;

public sealed class CreateAttributeCommandHandlerValidation : AbstractValidator<CreateAttributeCommand>
{
    public CreateAttributeCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required")
            .MaximumLength(160).WithMessage("Attribute name is too long")
            .MustAsync(async (name, cancellationToken) =>
            {
                var normalizedName = TextNormalizer.Normalize(name);
                return !await context.ProductAttributes.AnyAsync(
                    x => x.NormalizedName == normalizedName,
                    cancellationToken);
            })
            .WithMessage("Attribute name already exists");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Attribute type is invalid");

        RuleFor(x => x.Unit)
            .MaximumLength(32).WithMessage("Attribute unit is too long");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(x => x.Value)
                    .NotEmpty().WithMessage("Option value is required")
                    .MaximumLength(160).WithMessage("Option value is too long");
            });

        When(x => x.Type == AttributeType.Select, () =>
        {
            RuleFor(x => x.Options)
                .Must(options => options is null || options
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => TextNormalizer.Normalize(x.Value))
                    .Distinct()
                    .Count() == options.Count(x => !string.IsNullOrWhiteSpace(x.Value)))
                .WithMessage("Option values must be unique");
        });
    }
}
