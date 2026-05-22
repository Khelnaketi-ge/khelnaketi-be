using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginCommandHandlerValidation : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandHandlerValidation()
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
