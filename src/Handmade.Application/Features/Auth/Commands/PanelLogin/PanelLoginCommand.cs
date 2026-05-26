using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.PanelLogin;

public sealed record PanelLoginCommand(string Email, string Password) : IRequest<TokensModel>;

public sealed class PanelLoginCommandHandler(IAuthService authService) : IRequestHandler<PanelLoginCommand, TokensModel>
{
    public async Task<TokensModel> Handle(PanelLoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.PanelLoginAsync(request.Email, request.Password, cancellationToken);
    }
}
