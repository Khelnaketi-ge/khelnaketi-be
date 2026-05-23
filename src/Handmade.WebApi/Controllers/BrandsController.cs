using Asp.Versioning;
using Handmade.Application.Features.Brands.Commands.CreateBrand;
using Handmade.Domain.Enums;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BrandsController(ISender sender) : ApiController(sender)
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [HasPermission(default, isSuperAdminRequired: true)]
    public async Task<IActionResult> Create(
        [FromForm] CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/brands/{result.Id}", result);
    }
}
