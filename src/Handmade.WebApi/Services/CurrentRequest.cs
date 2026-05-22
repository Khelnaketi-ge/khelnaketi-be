using Handmade.Application.Interfaces;

namespace Handmade.WebApi.Services;

public class CurrentRequest(IHttpContextAccessor httpContextAccessor) : ICurrentRequest
{
    public string? IpAddress
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent
    {
        get
        {
            var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
            return string.IsNullOrWhiteSpace(userAgent) ? null : userAgent;
        }
    }
}
