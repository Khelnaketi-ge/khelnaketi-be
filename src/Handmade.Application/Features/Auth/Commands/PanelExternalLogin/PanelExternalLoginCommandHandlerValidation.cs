using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.PanelExternalLogin;

public sealed class PanelExternalLoginCommandHandlerValidation : AbstractValidator<PanelExternalLoginCommand>
{
    public PanelExternalLoginCommandHandlerValidation()
    {
        RuleFor(x => x.ProviderUserId)
            .NotEmpty()
            .WithMessage("External provider user id is required");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("External provider email is invalid");
        });
    }
}
