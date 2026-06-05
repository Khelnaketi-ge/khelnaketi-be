using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(int ProductId) : IRequest;

public sealed class DeleteProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .SingleOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ValidationException(nameof(request.ProductId), "Product was not found");
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
