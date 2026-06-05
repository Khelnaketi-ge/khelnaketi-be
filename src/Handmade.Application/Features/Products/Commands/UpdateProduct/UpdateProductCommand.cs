using System.Globalization;
using Handmade.Application.Common.Exceptions;
using Handmade.Application.Common.Localization;
using Handmade.Application.Common.Slugs;
using Handmade.Application.Features.Products.Commands.CreateProduct;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    int Id,
    int? BrandId,
    int CategoryId,
    string? Sku,
    decimal? Price,
    bool IsInStock,
    ProductStatus Status,
    IReadOnlyCollection<ProductAttributeValueInput>? AttributeValues,
    IReadOnlyCollection<ProductTranslationInput> Translations,
    IReadOnlyCollection<int>? ExistingImageIds,
    int? PrimaryExistingImageId,
    int? PrimaryImageIndex,
    IReadOnlyCollection<IFormFile>? Images) : IRequest<ProductDto>;

public sealed class UpdateProductCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private const string ProductImageFolder = "product-images";

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
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

        var product = await context.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Translations)
            .Include(x => x.Images)
                .ThenInclude(x => x.Image)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.ProductAttribute)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
            .SingleOrDefaultAsync(
                x => x.Id == request.Id && (isSuperAdmin || x.Brand.OwnerUserId == currentUser.Id.Value),
                cancellationToken);

        if (product is null)
        {
            throw new ValidationException(nameof(request.Id), "Product was not found");
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
            await using var transaction = await context.BeginTransactionAsync(cancellationToken);

            await context.ProductImages
                .Where(x => x.ProductId == product.Id && x.IsPrimary)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.IsPrimary, false),
                    cancellationToken);

            foreach (var productImage in product.Images)
            {
                productImage.IsPrimary = false;
            }

            product.CategoryId = category.Id;
            product.Category = category;
            if (request.BrandId.HasValue && isSuperAdmin)
            {
                var brandExists = await context.Brands.AnyAsync(
                    x => x.Id == request.BrandId.Value,
                    cancellationToken);

                if (!brandExists)
                {
                    throw new ValidationException(nameof(request.BrandId), "Brand was not found");
                }

                product.BrandId = request.BrandId.Value;
            }

            product.Sku = NormalizeOptional(request.Sku);
            product.Price = request.Price;
            product.IsInStock = request.IsInStock;
            product.Status = request.Status;

            foreach (var translation in product.Translations.ToList())
            {
                context.ProductTranslations.Remove(translation);
            }

            product.Translations.Clear();
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

            foreach (var attributeValue in product.AttributeValues.ToList())
            {
                context.ProductAttributeValues.Remove(attributeValue);
            }

            product.AttributeValues.Clear();
            foreach (var attributeValue in productAttributeValues)
            {
                product.AttributeValues.Add(attributeValue);
            }

            var keptImageIds = (request.ExistingImageIds ?? [])
                .ToHashSet();

            foreach (var productImage in product.Images.ToList()
                         .Where(productImage => !keptImageIds.Contains(productImage.Id)))
            {
                context.ProductImages.Remove(productImage);
                product.Images.Remove(productImage);
            }

            var newImages = (request.Images ?? []).Where(x => x.Length > 0).ToList();
            var newProductImages = new List<ProductImage>();
            var primaryImageIndex = request.PrimaryImageIndex.GetValueOrDefault(0);

            if (primaryImageIndex < 0 || primaryImageIndex >= newImages.Count)
            {
                primaryImageIndex = 0;
            }

            foreach (var file in newImages)
            {
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

                var productImage = new ProductImage
                {
                    ImageId = imageAsset.Id,
                    Image = imageAsset,
                    IsPrimary = false
                };

                product.Images.Add(productImage);
                newProductImages.Add(productImage);
            }

            var orderedImages = product.Images
                .OrderBy(x => x.Id == 0 ? int.MaxValue : x.Order)
                .ThenBy(x => x.Id)
                .ToList();

            var primaryImage = orderedImages.FirstOrDefault(x => request.PrimaryExistingImageId.HasValue && x.Id == request.PrimaryExistingImageId.Value)
                ?? newProductImages.ElementAtOrDefault(primaryImageIndex)
                ?? orderedImages.FirstOrDefault();

            for (var index = 0; index < orderedImages.Count; index++)
            {
                orderedImages[index].Order = index;
                orderedImages[index].IsPrimary = false;
            }

            await context.SaveChangesAsync(cancellationToken);

            if (primaryImage is not null)
            {
                primaryImage.IsPrimary = true;
                await context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            using var scope = new MapContextScope();

            scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

            return mapper.Map<ProductDto>(product);
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
        UpdateProductCommand request,
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
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integerValue))
                {
                    throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} must be an integer");
                }

                return integerValue.ToString(CultureInfo.InvariantCulture);
            case AttributeType.Boolean:
                if (!bool.TryParse(value, out var boolValue))
                {
                    throw new ValidationException($"attributes.{attribute.Id}", $"{GetAttributeName(attribute)} must be true or false");
                }

                return boolValue.ToString();
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
