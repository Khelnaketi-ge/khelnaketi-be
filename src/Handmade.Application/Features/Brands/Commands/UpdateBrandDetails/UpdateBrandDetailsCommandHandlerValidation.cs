using FluentValidation;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Commands.UpdateBrandDetails;

public sealed class UpdateBrandDetailsCommandHandlerValidation : AbstractValidator<UpdateBrandDetailsCommand>
{
    public UpdateBrandDetailsCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Brand id is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(160).WithMessage("Brand name is too long")
            .MustAsync(async (command, name, cancellationToken) =>
            {
                var normalizedName = TextNormalizer.Normalize(name);
                return !await context.Brands.AnyAsync(
                    x => x.Id != command.BrandId && x.NormalizedName == normalizedName,
                    cancellationToken);
            })
            .WithMessage("Brand name is already registered");

        RuleFor(x => x.LegalName)
            .MaximumLength(200).WithMessage("Legal name is too long");
    }
}
