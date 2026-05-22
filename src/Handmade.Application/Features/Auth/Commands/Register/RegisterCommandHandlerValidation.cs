using FluentValidation;
using Handmade.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandlerValidation : AbstractValidator<RegisterCommand>
{
    public RegisterCommandHandlerValidation(IApplicationDbContext context)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email address is invalid")
            .MaximumLength(320).WithMessage("Email address is too long")
            .MustAsync(async (email, cancellationToken) =>
            {
                var normalizedEmail = NormalizeEmail(email);
                return !await context.Users.AnyAsync(
                    x => x.NormalizedEmail == normalizedEmail
                         || x.Email.Trim().ToUpper() == normalizedEmail,
                    cancellationToken);
            })
            .WithMessage("Email address is already registered");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Password and confirm password do not match");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
