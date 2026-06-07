using Asp.Versioning;
using Handmade.Application.Features.Seo.Queries.GetBrandBySlug;
using Handmade.Application.Features.Seo.Queries.GetCategoryBySlug;
using Handmade.Application.Features.Seo.Queries.GetProductBySlug;
using Handmade.Application.Features.Seo.Queries.GetSeoBrands;
using Handmade.Application.Features.Seo.Queries.GetSeoCategories;
using Handmade.Application.Features.Seo.Queries.GetSeoProducts;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seo")]
public class SeoController(ISender sender) : ApiController(sender)
{
    [HttpGet("products/{slug}")]
    public async Task<IActionResult> GetProductBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductBySlugQuery(slug), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("categories/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoryBySlugQuery(slug), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("brands/{slug}")]
    public async Task<IActionResult> GetBrandBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBrandBySlugQuery(slug), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetSeoProductsQuery(), cancellationToken));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetSeoCategoriesQuery(), cancellationToken));
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetSeoBrandsQuery(), cancellationToken));
    }
}
