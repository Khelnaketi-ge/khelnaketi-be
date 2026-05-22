using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandlerValidation : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandHandlerValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is invalid");
    }
}
