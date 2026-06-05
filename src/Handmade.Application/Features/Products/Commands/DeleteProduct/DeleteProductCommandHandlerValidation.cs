using FluentValidation;

namespace Handmade.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandlerValidation : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandHandlerValidation()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
    }
}
