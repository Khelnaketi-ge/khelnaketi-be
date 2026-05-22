using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshCommand(string AccessToken, string RefreshToken) : IRequest<TokensModel>;

public sealed class RefreshCommandHandler(IAuthService authService) : IRequestHandler<RefreshCommand, TokensModel>
{
    public async Task<TokensModel> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        return await authService.RefreshAsync(request.AccessToken, request.RefreshToken, cancellationToken);
    }
}
