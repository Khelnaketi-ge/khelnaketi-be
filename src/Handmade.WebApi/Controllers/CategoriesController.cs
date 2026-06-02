using Asp.Versioning;
using Handmade.Application.Features.Categories.Commands.CreateCategory;
using Handmade.Application.Features.Categories.Commands.DeleteCategory;
using Handmade.Application.Features.Categories.Commands.LinkCategoryAttribute;
using Handmade.Application.Features.Categories.Commands.UnlinkCategoryAttribute;
using Handmade.Application.Features.Categories.Commands.UpdateCategory;
using Handmade.Application.Features.Categories.Commands.UpdateCategoryAttribute;
using Handmade.Application.Features.Categories.Commands.UpdateHomeCategory;
using Handmade.Application.Features.Categories.Commands.UpdateHomeCategoryImage;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Application.Features.Categories.Queries.GetHomeCategories;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[HasPermission(isSuperAdminRequired: true)]
public class CategoriesController(ISender sender) : ApiController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetCategoriesQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/categories/{result.Id}", result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("home")]
    public async Task<IActionResult> GetHome(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetHomeCategoriesQuery(), cancellationToken));
    }

    [HttpPut("home")]
    public async Task<IActionResult> UpdateHome(
        [FromBody] UpdateHomeCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPut("home/image")]
    public async Task<IActionResult> UpdateHomeImage(
        [FromForm] int homeCategoryId,
        [FromForm] IFormFile image,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new UpdateHomeCategoryImageCommand(homeCategoryId, image), cancellationToken));
    }

    [HttpPost("attributes")]
    public async Task<IActionResult> LinkAttribute(
        [FromBody] LinkCategoryAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return Created($"/api/v1/categories/attributes/{result.Id}", result);
    }

    [HttpPut("attributes")]
    public async Task<IActionResult> UpdateAttribute(
        [FromBody] UpdateCategoryAttributeCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete("attributes")]
    public async Task<IActionResult> UnlinkAttribute(
        [FromBody] UnlinkCategoryAttributeCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }
}
