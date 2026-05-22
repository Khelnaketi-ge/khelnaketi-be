using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandlerValidation : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandHandlerValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be 6 digits")
            .Matches("^[0-9]+$").WithMessage("Verification code must contain only digits");
    }
}
