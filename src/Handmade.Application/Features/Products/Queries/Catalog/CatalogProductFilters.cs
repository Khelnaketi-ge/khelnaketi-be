namespace Handmade.Application.Features.Products.Queries.Catalog;

public sealed record CatalogProductFilters(
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyCollection<string> Categories,
    IReadOnlyCollection<string> Brands,
    IReadOnlyCollection<string> Attributes);

public sealed record CatalogFilterOptionDto(
    string Label,
    string Value,
    int Count);

public sealed record CatalogAttributeFilterDto(
    string Label,
    string Value,
    IReadOnlyCollection<CatalogFilterOptionDto> Options);

public sealed record CatalogFiltersDto(
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyCollection<CatalogFilterOptionDto> Categories,
    IReadOnlyCollection<CatalogAttributeFilterDto> Attributes,
    IReadOnlyCollection<CatalogFilterOptionDto> Brands);
