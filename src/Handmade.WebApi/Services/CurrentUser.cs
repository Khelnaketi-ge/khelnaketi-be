using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Auth;

namespace Handmade.WebApi.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int? Id
    {
        get
        {
            var strId = httpContextAccessor.HttpContext?.User?.FindFirst(Claims.Id)?.Value;
            return int.TryParse(strId, out var id) ? id : null;
        }
    }

    public Guid? SessionId
    {
        get
        {
            var strId = httpContextAccessor.HttpContext?.User?.FindFirst(Claims.SessionId)?.Value;
            return Guid.TryParse(strId, out var id) ? id : null;
        }
    }
}
