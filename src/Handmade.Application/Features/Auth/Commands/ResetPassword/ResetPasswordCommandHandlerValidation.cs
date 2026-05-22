using FluentValidation;

namespace Handmade.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandlerValidation : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandHandlerValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is invalid");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(6)
            .WithMessage("Code must be 6 digits");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Password and confirm password do not match");
    }
}
