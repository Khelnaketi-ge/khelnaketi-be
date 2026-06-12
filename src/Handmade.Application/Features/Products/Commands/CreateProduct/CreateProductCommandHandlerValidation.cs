using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandlerValidation : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandHandlerValidation(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        TranslationValidation.ValidateProductTranslations(RuleFor(x => x.Translations));

        RuleFor(x => x.Sku)
            .MaximumLength(80).WithMessage("SKU is too long")
            .MustAsync(async (command, sku, cancellationToken) =>
                !await SkuExistsAsync(context, currentUser, command, sku, cancellationToken))
            .WithMessage("This SKU already exists");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).When(x => x.Price.HasValue).WithMessage("Price cannot be negative");

        RuleFor(x => x.DiscountPrice)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountPrice.HasValue).WithMessage("Discount price cannot be negative")
            .LessThan(x => x.Price).When(x => x.DiscountPrice.HasValue && x.Price.HasValue).WithMessage("Discount price must be lower than price");

        RuleFor(x => x.DiscountPercent)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountPercent.HasValue).WithMessage("Discount percent cannot be negative")
            .LessThanOrEqualTo(100).When(x => x.DiscountPercent.HasValue).WithMessage("Discount percent cannot be greater than 100");

        RuleFor(x => x)
            .Must(x => !(x.DiscountPrice.HasValue && x.DiscountPercent.HasValue))
            .WithMessage("Use either discount price or discount percent");

        RuleFor(x => x)
            .Must(x => x.Price.HasValue || (!x.DiscountPrice.HasValue && !x.DiscountPercent.HasValue))
            .WithMessage("Price is required when discount is set");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Product status is invalid")
            .Must(x => x is ProductStatus.Draft or ProductStatus.Active or ProductStatus.Archived)
            .WithMessage("Product status is invalid");

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

    private static async Task<bool> SkuExistsAsync(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        CreateProductCommand command,
        string? sku,
        CancellationToken cancellationToken)
    {
        var normalizedSku = NormalizeOptional(sku);

        if (normalizedSku is null || currentUser.Id is null)
        {
            return false;
        }

        var currentUserAccessLevel = await context.Users
            .Where(x => x.Id == currentUser.Id.Value)
            .Select(x => x.AccessLevel)
            .SingleOrDefaultAsync(cancellationToken);
        var isSuperAdmin = currentUserAccessLevel == AccessLevel.SuperAdmin;
        var brandId = command.BrandId.HasValue && isSuperAdmin
            ? command.BrandId.Value
            : await context.Brands
                .Where(x => x.OwnerUserId == currentUser.Id.Value)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync(cancellationToken);

        return brandId.HasValue
            && await context.Products.AnyAsync(
                x => x.BrandId == brandId.Value && x.Sku == normalizedSku,
                cancellationToken);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
