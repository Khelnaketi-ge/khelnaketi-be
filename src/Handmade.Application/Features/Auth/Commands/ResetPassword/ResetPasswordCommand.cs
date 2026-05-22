using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmNewPassword) : IRequest;

public sealed class ResetPasswordCommandHandler(IAuthService authService)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(
            request.Email,
            request.Code,
            request.NewPassword,
            request.ConfirmNewPassword,
            cancellationToken);
    }
}
