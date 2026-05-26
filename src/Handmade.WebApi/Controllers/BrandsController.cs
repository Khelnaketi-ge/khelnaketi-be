using Asp.Versioning;
using Handmade.Application.Features.Brands.Commands.CreateBrand;
using Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;
using Handmade.Application.Features.Brands.Commands.UpdateBrandDetails;
using Handmade.Application.Features.Brands.Commands.UpdateMyBrandLogo;
using Handmade.Application.Features.Brands.Queries.GetBrands;
using Handmade.Application.Features.Brands.Queries.GetMyBrand;
using Handmade.Infrastructure.Auth.Policies;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BrandsController(ISender sender) : ApiController(sender)
{
    public sealed record UpdateBrandDetailsRequest(string Name, string? LegalName);

    [HttpGet]
    [HasPermission(isSuperAdminRequired: true)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetBrandsQuery(), cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMyBrandQuery(), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{brandId:int}/logo")]
    [Consumes("multipart/form-data")]
    [HasPermission(brandOwnerRequired: true)]
    public async Task<IActionResult> UpdateMyLogo(
        [FromRoute] int brandId,
        [FromForm] IFormFile logo,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new UpdateMyBrandLogoCommand(brandId, logo), cancellationToken));
    }

    [HttpPut("{brandId:int}")]
    [HasPermission(brandOwnerRequired: true)]
    public async Task<IActionResult> UpdateDetails(
        [FromRoute] int brandId,
        [FromBody] UpdateBrandDetailsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(
            new UpdateBrandDetailsCommand(brandId, request.Name, request.LegalName),
            cancellationToken));
    }

    [HttpPut("{brandId:int}/contacts")]
    [HasPermission(brandOwnerRequired: true)]
    public async Task<IActionResult> UpdateContacts(
        [FromRoute] int brandId,
        [FromBody] UpdateBrandContactsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(
            new UpdateBrandContactsCommand(
                brandId,
                request.PhoneNumbers,
                request.EmailAddresses,
                request.Addresses),
            cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [HasPermission(isSuperAdminRequired: true)]
    public async Task<IActionResult> Create(
        [FromForm] CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created($"/api/v1/brands/{result.Id}", result);
    }

    public sealed record UpdateBrandContactsRequest(
        IReadOnlyCollection<BrandPhoneNumberInput> PhoneNumbers,
        IReadOnlyCollection<BrandEmailAddressInput> EmailAddresses,
        IReadOnlyCollection<BrandAddressInput> Addresses);
}
