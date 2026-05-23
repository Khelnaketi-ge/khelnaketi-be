using FluentValidation;
using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandlerValidation : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(160).WithMessage("Brand name is too long")
            .MustAsync(async (name, cancellationToken) =>
            {
                var normalizedName = TextNormalizer.Normalize(name);
                return !await context.Brands.AnyAsync(
                    x => x.NormalizedName == normalizedName,
                    cancellationToken);
            })
            .WithMessage("Brand name is already registered");

        RuleFor(x => x.OwnerUserId)
            .GreaterThan(0).WithMessage("Owner user id is required")
            .MustAsync(async (ownerUserId, cancellationToken) =>
                await context.Users.AnyAsync(x => x.Id == ownerUserId, cancellationToken))
            .WithMessage("Owner user was not found");

        RuleFor(x => x.LegalName)
            .MaximumLength(200).WithMessage("Legal name is too long");

        When(x => x.Logo is not null, () =>
        {
            RuleFor(x => x.Logo!.FileName)
                .NotEmpty().WithMessage("Logo file name is required")
                .MaximumLength(255).WithMessage("Logo file name is too long");

            RuleFor(x => x.Logo!.ContentType)
                .NotEmpty().WithMessage("Logo content type is required");

            RuleFor(x => x.Logo!.Length)
                .GreaterThan(0).WithMessage("Logo size is required");
        });
    }

}
