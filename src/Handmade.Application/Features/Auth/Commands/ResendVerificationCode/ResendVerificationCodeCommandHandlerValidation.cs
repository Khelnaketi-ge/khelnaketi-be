using FluentValidation;
using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Auth.Commands.ResendVerificationCode;

public sealed class ResendVerificationCodeCommandHandlerValidation : AbstractValidator<ResendVerificationCodeCommand>
{
    public ResendVerificationCodeCommandHandlerValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Purpose)
            .IsInEnum()
            .Must(x => x is VerificationCodePurpose.EmailVerification or VerificationCodePurpose.PasswordReset)
            .WithMessage("Verification code purpose is invalid");
    }
}
