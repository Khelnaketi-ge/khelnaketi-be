using Asp.Versioning;
using Handmade.Application.Features.Cart.Commands.AddCartItem;
using Handmade.Application.Features.Cart.Commands.ClearCart;
using Handmade.Application.Features.Cart.Commands.DecrementCartItem;
using Handmade.Application.Features.Cart.Commands.RemoveCartItem;
using Handmade.Application.Features.Cart.Queries.GetCartItems;
using Handmade.Application.Features.Cart.Queries.GetCartSummary;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[HasPermission]
public class CartController(ISender sender) : ApiController(sender)
{
    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken = default)
    {
        return Ok(await Sender.Send(new GetCartItemsQuery(), cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken = default)
    {
        return Ok(await Sender.Send(new GetCartSummaryQuery(), cancellationToken));
    }

    [HttpPost("items/{productId:int}")]
    public async Task<IActionResult> Add(
        [FromRoute] int productId,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new AddCartItemCommand(productId), cancellationToken));
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> Remove(
        [FromRoute] int productId,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new RemoveCartItemCommand(productId), cancellationToken));
    }

    [HttpPost("items/{productId:int}/decrement")]
    public async Task<IActionResult> Decrement(
        [FromRoute] int productId,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new DecrementCartItemCommand(productId), cancellationToken));
    }

    [HttpDelete("items")]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new ClearCartCommand(), cancellationToken));
    }
}
