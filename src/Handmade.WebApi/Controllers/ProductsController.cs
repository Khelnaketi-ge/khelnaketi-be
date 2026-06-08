using Asp.Versioning;
using Handmade.Application.Features.Products.Commands.CreateProduct;
using Handmade.Application.Features.Products.Commands.DeleteProduct;
using Handmade.Application.Features.Products.Commands.UpdateProduct;
using Handmade.Application.Features.Products.Commands.UpdateProductStatus;
using Handmade.Application.Features.Products.Queries.Catalog;
using Handmade.Application.Features.Products.Queries.GetAdminProducts;
using Handmade.Application.Features.Products.Queries.GetCatalogFilters;
using Handmade.Application.Features.Products.Queries.GetCatalogProducts;
using Handmade.Application.Features.Products.Queries.GetCategoryProductsBySlug;
using Handmade.Application.Features.Products.Queries.GetMyProducts;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(ISender sender) : ApiController(sender)
{
    [HttpGet("me")]
    [HasPermission(brandOwnerRequired: true)]
    public async Task<IActionResult> GetMine(
        [FromQuery] ProductStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await Sender.Send(new GetMyProductsQuery(status, search, page, pageSize), cancellationToken));
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalogProducts(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string[] category,
        [FromQuery] string[] brand,
        [FromQuery] string[] attribute,
        [FromQuery] CatalogProductSort sortBy = CatalogProductSort.Newest,
        CancellationToken cancellationToken = default)
    {
        return Ok(await Sender.Send(
            new GetCatalogProductsQuery(new CatalogProductFilters(
                search,
                minPrice,
                maxPrice,
                category,
                brand,
                attribute,
                sortBy)),
            cancellationToken));
    }

    [HttpGet("catalog/filters")]
    public async Task<IActionResult> GetCatalogFilters(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string[] category,
        [FromQuery] string[] brand,
        [FromQuery] string[] attribute,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(
            new GetCatalogFiltersQuery(new CatalogProductFilters(
                search,
                minPrice,
                maxPrice,
                category,
                brand,
                attribute)),
            cancellationToken));
    }

    [HttpGet("categories/{slug}")]
    public async Task<IActionResult> GetCategoryProductsBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetCategoryProductsBySlugQuery(slug), cancellationToken));
    }

    [HttpGet("admin")]
    [HasPermission(isSuperAdminRequired: true)]
    public async Task<IActionResult> GetAdmin(
        [FromQuery] int? brandId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await Sender.Send(new GetAdminProductsQuery(brandId, search, page, pageSize), cancellationToken));
    }

    [HttpPost]
    [HasPermission(brandOwnerRequired: true)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/products/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(brandOwnerRequired: true)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromForm] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command with { Id = id }, cancellationToken));
    }

    [HttpPatch("{id:int}/status")]
    [HasPermission(isSuperAdminRequired: true)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] int id,
        [FromBody] UpdateProductStatusCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command with { ProductId = id }, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [HasPermission(isSuperAdminRequired: true)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
