using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Handmade.WebApi.Infrastructure;

[ApiController]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;
}