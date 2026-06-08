namespace Handmade.Application.Features.Products.Queries.Catalog;

public sealed record CatalogProductFilters(
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyCollection<string> Categories,
    IReadOnlyCollection<string> Brands,
    IReadOnlyCollection<string> Attributes,
    CatalogProductSort SortBy = CatalogProductSort.Newest);

public sealed record CatalogFilterOptionDto(
    string Label,
    string Value,
    int Count);

public sealed record CatalogCategoryFilterDto(
    string Label,
    string Value,
    int Count,
    IReadOnlyCollection<CatalogCategoryFilterDto> Children);

public sealed record CatalogAttributeFilterDto(
    string Label,
    string Value,
    IReadOnlyCollection<CatalogFilterOptionDto> Options);

public sealed record CatalogFiltersDto(
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyCollection<CatalogCategoryFilterDto> Categories,
    IReadOnlyCollection<CatalogAttributeFilterDto> Attributes,
    IReadOnlyCollection<CatalogFilterOptionDto> Brands);
