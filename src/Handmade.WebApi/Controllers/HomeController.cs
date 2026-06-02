using Asp.Versioning;
using Handmade.Application.Features.Categories.Queries.GetHomeCategories;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HomeController(ISender sender) : ApiController(sender)
{
    [HttpGet("popular-categories")]
    public async Task<IActionResult> GetPopularCategories(CancellationToken cancellationToken)
    {
        return Ok(await Sender.Send(new GetHomeCategoriesQuery(), cancellationToken));
    }
}
