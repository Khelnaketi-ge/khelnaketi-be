using FluentValidation;

namespace Handmade.Application.Features.Brands.Commands.UpdateBrandContacts;

public sealed class UpdateBrandContactsCommandHandlerValidation : AbstractValidator<UpdateBrandContactsCommand>
{
    public UpdateBrandContactsCommandHandlerValidation()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Brand id is required");

        RuleFor(x => x.PhoneNumbers)
            .NotNull().WithMessage("Phone numbers are required");

        RuleForEach(x => x.PhoneNumbers).ChildRules(phoneNumber =>
        {
            phoneNumber.RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(32).WithMessage("Phone number is too long");

            phoneNumber.RuleFor(x => x.Label)
                .MaximumLength(80).WithMessage("Phone number label is too long");
        });

        RuleFor(x => x.PhoneNumbers)
            .Must(x => x.Count(phoneNumber => phoneNumber.IsPrimary && phoneNumber.IsActive) <= 1)
            .WithMessage("Only one active primary phone number is allowed");

        RuleFor(x => x.EmailAddresses)
            .NotNull().WithMessage("Email addresses are required");

        RuleForEach(x => x.EmailAddresses).ChildRules(emailAddress =>
        {
            emailAddress.RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Email address is invalid")
                .MaximumLength(320).WithMessage("Email address is too long");

            emailAddress.RuleFor(x => x.Label)
                .MaximumLength(80).WithMessage("Email address label is too long");
        });

        RuleFor(x => x.EmailAddresses)
            .Must(x => x.Count(emailAddress => emailAddress.IsPrimary && emailAddress.IsActive) <= 1)
            .WithMessage("Only one active primary email address is allowed");

        RuleFor(x => x.Addresses)
            .NotNull().WithMessage("Addresses are required");

        RuleForEach(x => x.Addresses).ChildRules(address =>
        {
            address.RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(120).WithMessage("City is too long");

            address.RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(250).WithMessage("Address line 1 is too long");

            address.RuleFor(x => x.AddressLine2)
                .MaximumLength(250).WithMessage("Address line 2 is too long");

            address.RuleFor(x => x.PostalCode)
                .MaximumLength(32).WithMessage("Postal code is too long");

            address.RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude is invalid");

            address.RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude is invalid");
        });

        RuleFor(x => x.Addresses)
            .Must(x => x.Count(address => address.IsPrimary && address.IsActive) <= 1)
            .WithMessage("Only one active primary address is allowed");
    }
}
