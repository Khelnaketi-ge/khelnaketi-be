using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandlerValidation : AbstractValidator<LoginCommand>
{
    public LoginCommandHandlerValidation()
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
