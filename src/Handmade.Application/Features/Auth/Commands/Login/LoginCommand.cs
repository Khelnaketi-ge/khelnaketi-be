using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<TokensModel>;

public sealed class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, TokensModel>
{
    public async Task<TokensModel> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
