using Handmade.Application.Interfaces;
using MediatR;

namespace Handmade.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName, 
    string Email,
    string Password,
    string ConfirmPassword) : IRequest;

public sealed class RegisterCommandHandler(IAuthService authService) : IRequestHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        await authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ConfirmPassword,
            cancellationToken);
    }
}
