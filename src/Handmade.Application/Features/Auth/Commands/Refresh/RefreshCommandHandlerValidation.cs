using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshCommandHandlerValidation : AbstractValidator<RefreshCommand>
{
    public RefreshCommandHandlerValidation()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
