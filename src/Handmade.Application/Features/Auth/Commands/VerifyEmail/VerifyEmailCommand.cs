using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string Code) : IRequest<TokensModel>;

public sealed class VerifyEmailCommandHandler(IAuthService authService) : IRequestHandler<VerifyEmailCommand, TokensModel>
{
    public async Task<TokensModel> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return await authService.VerifyEmailCodeAsync(request.Email, request.Code, cancellationToken);
    }
}
