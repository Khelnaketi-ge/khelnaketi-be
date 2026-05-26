using Handmade.Application.Common.Models.Auth;

namespace Handmade.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<TokensModel> ExternalLoginAsync(ExternalLoginModel login, CancellationToken cancellationToken);
    Task<TokensModel> PanelExternalLoginAsync(ExternalLoginModel login, CancellationToken cancellationToken);
}
