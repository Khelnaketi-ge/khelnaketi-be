using FluentValidation;
using Handmade.Application.Common.Localization;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandlerValidation : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandHandlerValidation(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product is required");

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
        UpdateProductCommand command,
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
        var product = await context.Products
            .Where(x => x.Id == command.Id && (isSuperAdmin || x.Brand.OwnerUserId == currentUser.Id.Value))
            .Select(x => new
            {
                x.Id,
                x.BrandId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return false;
        }

        var brandId = command.BrandId.HasValue && isSuperAdmin
            ? command.BrandId.Value
            : product.BrandId;

        return await context.Products.AnyAsync(
            x => x.Id != command.Id && x.BrandId == brandId && x.Sku == normalizedSku,
            cancellationToken);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
