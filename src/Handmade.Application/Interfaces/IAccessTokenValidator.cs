using System.Security.Claims;
using Handmade.Application.Common.Models.Auth;

namespace Handmade.Application.Interfaces;

public interface IAccessTokenValidator
{
    Task<AccessTokenValidationResult> ValidateAsync(
        ClaimsPrincipal? principal,
        CancellationToken cancellationToken);
}
