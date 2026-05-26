using Asp.Versioning;
using Handmade.Application.Features.Categories.Commands.CreateCategory;
using Handmade.Application.Features.Categories.Commands.LinkCategoryAttribute;
using Handmade.Application.Features.Categories.Commands.UpdateCategoryAttribute;
using Handmade.Application.Features.Categories.Queries.GetCategories;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
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
}
