using Asp.Versioning;
using Handmade.Application.Features.Users.Commands.EditUser;
using Handmade.Application.Features.Users.Queries.GetUserById;
using Handmade.Application.Features.Users.Queries.GetUsers;
using Handmade.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(ISender sender): ApiController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await Sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }
    
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserById(
        [FromRoute] int userId, CancellationToken cancellationToken)
    {
        var users = await Sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return Ok(users);
    }

    [HttpPut]
    public async Task<IActionResult> EditUser(
        EditUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }
}