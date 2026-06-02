using FluentValidation;

namespace Handmade.Application.Features.Categories.Commands.UpdateHomeCategoryImage;

public sealed class UpdateHomeCategoryImageCommandValidator
    : AbstractValidator<UpdateHomeCategoryImageCommand>
{
    public UpdateHomeCategoryImageCommandValidator()
    {
        RuleFor(x => x.HomeCategoryId)
            .GreaterThan(0);

        RuleFor(x => x.Image)
            .NotNull();

        RuleFor(x => x.Image.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .When(x => x.Image is not null);

        RuleFor(x => x.Image.ContentType)
            .NotEmpty()
            .Must(contentType => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only image files are allowed")
            .When(x => x.Image is not null);

        RuleFor(x => x.Image.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .When(x => x.Image is not null);
    }
}
