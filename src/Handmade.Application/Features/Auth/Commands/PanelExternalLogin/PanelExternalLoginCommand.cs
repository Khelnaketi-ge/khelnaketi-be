using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.PanelExternalLogin;

public sealed record PanelExternalLoginCommand(
    Provider Provider,
    string ProviderUserId,
    string? Email,
    bool EmailVerified,
    string? DisplayName) : IRequest<TokensModel>;

public sealed class PanelExternalLoginCommandHandler(IGoogleAuthService googleAuthService)
    : IRequestHandler<PanelExternalLoginCommand, TokensModel>
{
    public async Task<TokensModel> Handle(PanelExternalLoginCommand request, CancellationToken cancellationToken)
    {
        return await googleAuthService.PanelExternalLoginAsync(
            new ExternalLoginModel(
                request.Provider,
                request.ProviderUserId,
                request.Email,
                request.EmailVerified,
                request.DisplayName),
            cancellationToken);
    }
}
