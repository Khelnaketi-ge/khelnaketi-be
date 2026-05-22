using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : IRequest;

public sealed class RequestPasswordResetCommandHandler(IAuthService authService)
    : IRequestHandler<RequestPasswordResetCommand>
{
    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        await authService.RequestPasswordResetAsync(request.Email, cancellationToken);
    }
}
