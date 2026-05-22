using Handmade.Application.Common.Models.Auth;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.ExternalLogin;

public sealed record ExternalLoginCommand(
    Provider Provider,
    string ProviderUserId,
    string? Email,
    bool EmailVerified,
    string? DisplayName) : IRequest<TokensModel>;

public sealed class ExternalLoginCommandHandler(IGoogleAuthService googleAuthService)
    : IRequestHandler<ExternalLoginCommand, TokensModel>
{
    public async Task<TokensModel> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        return await googleAuthService.ExternalLoginAsync(
            new ExternalLoginModel(
                request.Provider,
                request.ProviderUserId,
                request.Email,
                request.EmailVerified,
                request.DisplayName),
            cancellationToken);
    }
}
