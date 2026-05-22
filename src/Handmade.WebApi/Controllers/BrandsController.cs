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
    [HasPermission(Permissions.None, isSuperAdminRequired: true)]
    public async Task<IActionResult> Create([FromForm] CreateBrandRequest request, CancellationToken cancellationToken)
    {
        await using var logoStream = request.Logo?.OpenReadStream();

        var result = await Sender.Send(
            new CreateBrandCommand(
                request.Name,
                request.OwnerUserId,
                request.LegalName,
                logoStream is null || request.Logo is null
                    ? null
                    : new UploadFileInput(
                        logoStream,
                        request.Logo.FileName,
                        request.Logo.ContentType,
                        request.Logo.Length)),  
            cancellationToken);

        return Created($"/api/v1/brands/{result.Id}", result);
    }
}

public sealed class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
    public string? LegalName { get; set; }
    public IFormFile? Logo { get; set; }
}
