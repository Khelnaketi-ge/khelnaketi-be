using FluentValidation;
using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Products.Commands.UpdateProductStatus;

public sealed class UpdateProductStatusCommandHandlerValidation : AbstractValidator<UpdateProductStatusCommand>
{
    public UpdateProductStatusCommandHandlerValidation()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(x => x is ProductStatus.Draft or ProductStatus.Active or ProductStatus.Archived);
    }
}
