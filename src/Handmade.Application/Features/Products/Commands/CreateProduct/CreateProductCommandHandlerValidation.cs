using FluentValidation;
using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandlerValidation : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandHandlerValidation()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(180).WithMessage("Product name is too long");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description is too long");

        RuleFor(x => x.Sku)
            .MaximumLength(80).WithMessage("SKU is too long");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).When(x => x.Price.HasValue).WithMessage("Price cannot be negative");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Product status is invalid")
            .Must(x => x is ProductStatus.Draft or ProductStatus.Active).WithMessage("Product status is invalid");

        RuleForEach(x => x.Images)
            .ChildRules(image =>
            {
                image.RuleFor(x => x.FileName)
                    .NotEmpty().WithMessage("Image file name is required")
                    .MaximumLength(255).WithMessage("Image file name is too long");

                image.RuleFor(x => x.ContentType)
                    .NotEmpty().WithMessage("Image content type is required")
                    .Must(contentType => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Only image files are allowed");

                image.RuleFor(x => x.Length)
                    .GreaterThan(0).WithMessage("Image size is required")
                    .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("Image size must be 10 MB or less");
            });
    }
}
