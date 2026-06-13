using Handmade.Application.Interfaces;
using Handmade.Application.Features.Attributes.Models;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Mapster;

namespace Handmade.Application.Features.Products.Models;

public sealed class ProductDto : IMapFrom<Product>
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public DateTimeOffset Created { get; set; }

    public IReadOnlyCollection<ProductTranslationDto> Translations { get; set; } = [];
    public IReadOnlyCollection<ProductImageDto> Images { get; set; } = [];
    public IReadOnlyCollection<ProductAttributeValueDto> AttributeValues { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.BrandName, src => src.Brand.Name)
            .Map(dest => dest.CategoryName, src => src.Category.Translations
                .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Name)
            .Map(dest => dest.Name, src => src.Translations
                .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Title)
            .Map(dest => dest.Description, src => src.Translations
                .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Description)
            .Map(dest => dest.Images, src => src.Images
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Id))
            .Map(dest => dest.AttributeValues, src => src.AttributeValues
                .OrderBy(x => x.ProductAttribute.Translations
                    .FirstOrDefault(t => t.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Name));
    }
}

public sealed class ProductTranslationDto : IMapFrom<ProductTranslation>
{
    public string LanguageCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
}

public sealed class ProductImageDto : IMapFrom<ProductImage>
{
    public int Id { get; set; }
    public Guid ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
    public bool IsPrimary { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProductImage, ProductImageDto>()
            .Map(dest => dest.ImageUrl, src =>
                ((IImageStorageService)MapContext.Current!.Parameters[nameof(IImageStorageService)])
                    .GetPublicUrl(src.Image.ObjectKey));
    }
}

public sealed class ProductAttributeValueDto : IMapFrom<ProductAttributeValue>
{
    public int Id { get; set; }
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = null!;
    public AttributeType Type { get; set; }
    public string Value { get; set; } = null!;
    public int? OptionId { get; set; }
    public string? OptionValue { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProductAttributeValue, ProductAttributeValueDto>()
            .Map(dest => dest.AttributeId, src => src.ProductAttributeId)
            .Map(dest => dest.AttributeName, src => src.ProductAttribute.Translations
                .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Name)
            .Map(dest => dest.Type, src => src.ProductAttribute.Type)
            .Map(dest => dest.OptionId, src => src.AttributeOptionId)
            .Map(dest => dest.OptionValue, src => src.AttributeOption != null
                ? src.AttributeOption.Translations
                    .FirstOrDefault(x => x.LanguageCode == Common.Localization.LanguageCodes.Georgian)!.Value
                : null);
    }
}
