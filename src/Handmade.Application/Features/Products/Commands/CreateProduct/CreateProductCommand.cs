using System.Globalization;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    int? BrandId,
    int CategoryId,
    string? Sku,
    decimal? Price,
    decimal? DiscountPrice,
    decimal? DiscountPercent,
    bool IsInStock,
    ProductStatus Status,
    IReadOnlyCollection<ProductAttributeValueInput>? AttributeValues,
    IReadOnlyCollection<ProductTranslationInput> Translations,
    int? PrimaryImageIndex,
    IReadOnlyCollection<IFormFile>? Images) : IRequest<ProductDto>;

public sealed record ProductAttributeValueInput(
    int AttributeId,
    string? Value,
    int? OptionId);

public sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<CreateProductCommand, ProductDto>
{
    private const string ProductImageFolder = "product-images";

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var currentUserAccessLevel = await context.Users
            .Where(x => x.Id == currentUser.Id.Value)
            .Select(x => x.AccessLevel)
            .SingleOrDefaultAsync(cancellationToken);
        var isSuperAdmin = currentUserAccessLevel == AccessLevel.SuperAdmin;

        var brand = request.BrandId.HasValue && isSuperAdmin
            ? await context.Brands.SingleOrDefaultAsync(x => x.Id == request.BrandId.Value, cancellationToken)
            : await context.Brands.SingleOrDefaultAsync(x => x.OwnerUserId == currentUser.Id.Value, cancellationToken);

        if (brand is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.BrandOwnerRequired);
        }

        var category = await context.Categories
            .Include(x => x.Children)
            .Include(x => x.CategoryAttributes)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Translations)
            .Include(x => x.CategoryAttributes)
                .ThenInclude(x => x.ProductAttribute)
                    .ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new ValidationException(nameof(request.CategoryId), "Category was not found");
        }

        if (category.Children.Count > 0)
        {
            throw new ValidationException(nameof(request.CategoryId), "Products can only be added to leaf categories");
        }

        var productAttributeValues = BuildAttributeValues(request, category);
        var uploadedImages = new List<ImageUploadResult>();

        try
        {
            var product = new Product
            {
                BrandId = brand.Id,
                Brand = brand,
                CategoryId = category.Id,
                Category = category,
                Sku = NormalizeOptional(request.Sku),
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                DiscountPercent = request.DiscountPercent,
                IsInStock = request.IsInStock,
                Status = request.Status,
                AttributeValues = productAttributeValues
            };

            var images = (request.Images ?? []).Where(x => x.Length > 0).ToList();
            var primaryImageIndex = request.PrimaryImageIndex.GetValueOrDefault(0);

            if (primaryImageIndex < 0 || primaryImageIndex >= images.Count)
            {
                primaryImageIndex = 0;
            }

            for (var index = 0; index < images.Count; index++)
            {
                var file = images[index];
                await using var imageStream = file.OpenReadStream();
                var uploadedImage = await imageStorage.UploadAsync(
                    new ImageUploadRequest(
                        imageStream,
                        file.FileName,
                        file.ContentType,
                        file.Length,
                        ProductImageFolder),
                    cancellationToken);

                uploadedImages.Add(uploadedImage);

                var imageAsset = new ImageAsset
                {
                    Id = uploadedImage.Id,
                    BucketName = uploadedImage.BucketName,
                    ObjectKey = uploadedImage.ObjectKey,
                    OriginalFileName = uploadedImage.OriginalFileName,
                    ContentType = uploadedImage.ContentType,
                    SizeBytes = uploadedImage.SizeBytes,
                    UploadedByUserId = currentUser.Id.Value
                };

                context.ImageAssets.Add(imageAsset);

                product.Images.Add(new ProductImage
                {
                    ImageId = imageAsset.Id,
                    Image = imageAsset,
                    Order = index,
                    IsPrimary = index == primaryImageIndex
                });
            }

            context.Products.Add(product);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var input in request.Translations)
            {
                product.Translations.Add(new ProductTranslation
                {
                    ProductId = product.Id,
                    LanguageCode = LanguageCodes.Normalize(input.LanguageCode),
                    Title = input.Title.Trim(),
                    Slug = SlugGenerator.GenerateForEntity(input.Title, "p", product.Id, 220),
                    ShortDescription = NormalizeOptional(input.ShortDescription),
                    Description = NormalizeOptional(input.Description)
                });
            }

            await context.SaveChangesAsync(cancellationToken);

            using var scope = new MapContextScope();

            scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

            var mappedProduct = await context.Products
                .AsNoTracking()
                .IncludeProductDtoGraph()
                .SingleAsync(x => x.Id == product.Id, cancellationToken);

            return mapper.Map<ProductDto>(mappedProduct);
        }
        catch
        {
            foreach (var uploadedImage in uploadedImages)
            {
                await imageStorage.DeleteAsync(uploadedImage.ObjectKey, CancellationToken.None);
            }

            throw;
        }
    }

    private static List<ProductAttributeValue> BuildAttributeValues(
        CreateProductCommand request,
        Category category)
    {
        var inputsByAttributeId = (request.AttributeValues ?? [])
            .GroupBy(x => x.AttributeId)
            .ToDictionary(x => x.Key, x => x.Last());
        var values = new List<ProductAttributeValue>();

        foreach (var categoryAttribute in category.CategoryAttributes)
        {
            var attribute = categoryAttribute.ProductAttribute;
            inputsByAttributeId.TryGetValue(attribute.Id, out var input);

            var value = NormalizeOptional(input?.Value);
            var optionId = input?.OptionId;

            if (categoryAttribute.IsRequired && string.IsNullOrWhiteSpace(value) && optionId is null)
            {
                throw new ValidationException(
                    $"attributes.{attribute.Id}",
                    $"{GetAttributeName(attribute)} is required");
            }

            if (string.IsNullOrWhiteSpace(value) && optionId is null)
            {
                continue;
            }

            values.Add(new ProductAttributeValue
            {
                ProductAttributeId = attribute.Id,
                AttributeOptionId = ValidateAndGetOptionId(attribute, value, optionId),
                Value = NormalizeValue(attribute, value, optionId)
            });
        }

        return values;
    }

    private static int? ValidateAndGetOptionId(ProductAttribute attribute, string? value, int? optionId)
    {
        if (attribute.Type != AttributeType.Select)
        {
            return null;
        }

        if (optionId is null)
        {
            throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} option is required");
        }

        if (attribute.Options.All(x => x.Id != optionId.Value))
        {
            throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} option is invalid");
        }

        return optionId;
    }

    private static string NormalizeValue(ProductAttribute attribute, string? value, int? optionId)
    {
        switch (attribute.Type)
        {
            case AttributeType.Text:
                return value?.Trim() ?? string.Empty;
            case AttributeType.Integer:
                return !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue) 
                    ? throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} must be an integer") 
                    : integerValue.ToString(CultureInfo.InvariantCulture);

            case AttributeType.Boolean:
                return !bool.TryParse(value, out var boolValue) 
                    ? throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} must be true or false") 
                    : boolValue.ToString();

            case AttributeType.Select:
                return optionId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            default:
                throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} type is invalid");
        }
    }

    private static string GetAttributeName(ProductAttribute attribute) =>
        attribute.Translations
            .FirstOrDefault(x => x.LanguageCode == LanguageCodes.Georgian)?.Name
        ?? attribute.Translations.FirstOrDefault()?.Name
        ?? $"Attribute {attribute.Id}";

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
