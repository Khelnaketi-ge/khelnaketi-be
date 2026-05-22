using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.ResendVerificationCode;

public sealed record ResendVerificationCodeCommand(string Email, VerificationCodePurpose Purpose) : IRequest;

public sealed class ResendVerificationCodeCommandHandler(IAuthService authService)
    : IRequestHandler<ResendVerificationCodeCommand>
{
    public async Task Handle(ResendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        await authService.ResendVerificationCodeAsync(request.Email, request.Purpose, cancellationToken);
    }
}
