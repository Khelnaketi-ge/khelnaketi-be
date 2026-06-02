using FluentValidation;

namespace Handmade.Application.Features.Users.Commands.EditUser;

public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
{
    public EditUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invalid user id.");

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("First name is required.")
            .Must(x => x.Trim().Length <= 100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Last name is required.")
            .Must(x => x.Trim().Length <= 100)
            .WithMessage("Last name must not exceed 100 characters.");
    }
}