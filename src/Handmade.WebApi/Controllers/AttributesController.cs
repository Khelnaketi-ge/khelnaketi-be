using Asp.Versioning;
using Handmade.Application.Features.Attributes.Commands.CreateAttribute;
using Handmade.Application.Features.Attributes.Commands.CreateAttributeOption;
using Handmade.Application.Features.Attributes.Commands.DeleteAttribute;
using Handmade.Application.Features.Attributes.Commands.DeleteAttributeOption;
using Handmade.Application.Features.Attributes.Commands.ReorderAttributeOptions;
using Handmade.Application.Features.Attributes.Commands.UpdateAttribute;
using Handmade.Application.Features.Attributes.Commands.UpdateAttributeStatus;
using Handmade.Application.Features.Attributes.Commands.UpdateAttributeOption;
using Handmade.Application.Features.Attributes.Queries.GetAttributes;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[HasPermission(isSuperAdminRequired: true)]
public class AttributesController(ISender sender) : ApiController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetAttributesQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAttributeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/attributes/{result.Id}", result);
    }

    [HttpPatch("status")]
    public async Task<IActionResult> UpdateStatus(
        [FromBody] UpdateAttributeStatusCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateAttributeCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteAttributeCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("options")]
    public async Task<IActionResult> CreateOption(
        [FromBody] CreateAttributeOptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/attributes/options/{result.Id}", result);
    }

    [HttpPut("options")]
    public async Task<IActionResult> UpdateOption(
        [FromBody] UpdateAttributeOptionCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpPut("options/order")]
    public async Task<IActionResult> ReorderOptions(
        [FromBody] ReorderAttributeOptionsCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(command, cancellationToken));
    }

    [HttpDelete("options")]
    public async Task<IActionResult> DeleteOption(
        [FromBody] DeleteAttributeOptionCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }
}
