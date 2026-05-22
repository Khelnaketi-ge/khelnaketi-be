using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand : IRequest;

public sealed class LogoutCommandHandler(
    IAuthService authService,
    ICurrentUser currentUser) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null || currentUser.SessionId is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        await authService.LogoutAsync(currentUser.Id.Value, currentUser.SessionId.Value, cancellationToken);
    }
}
