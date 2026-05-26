using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.PanelLogin;

public sealed class PanelLoginCommandHandlerValidation : AbstractValidator<PanelLoginCommand>
{
    public PanelLoginCommandHandlerValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}
