using Asp.Versioning;
using Handmade.Application.Features.Products.Commands.CreateProduct;
using Handmade.Application.Features.Products.Commands.UpdateProduct;
using Handmade.Application.Features.Products.Queries.GetMyProducts;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[HasPermission(brandOwnerRequired: true)]
public class ProductsController(ISender sender) : ApiController(sender)
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetMyProductsQuery(), cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/products/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromForm] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command with { Id = id }, cancellationToken));
    }
}
