using FluentValidation;

namespace Handmade.Application.Features.Brands.Commands.UpdateMyBrandLogo;

public sealed class UpdateMyBrandLogoCommandHandlerValidation : AbstractValidator<UpdateMyBrandLogoCommand>
{
    public UpdateMyBrandLogoCommandHandlerValidation()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Brand id is required");

        RuleFor(x => x.Logo)
            .NotNull().WithMessage("Logo is required");

        When(x => x.Logo is not null, () =>
        {
            RuleFor(x => x.Logo.FileName)
                .NotEmpty().WithMessage("Logo file name is required")
                .MaximumLength(255).WithMessage("Logo file name is too long");

            RuleFor(x => x.Logo.ContentType)
                .NotEmpty().WithMessage("Logo content type is required");

            RuleFor(x => x.Logo.Length)
                .GreaterThan(0).WithMessage("Logo size is required");
        });
    }
}
