using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Products.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand(int ProductId, ProductStatus Status) : IRequest<ProductDto>;

public sealed class UpdateProductStatusCommandHandler(
    IApplicationDbContext context,
    IImageStorageService imageStorage,
    IMapper mapper) : IRequestHandler<UpdateProductStatusCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Images)
                .ThenInclude(x => x.Image)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.ProductAttribute)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeOption)
            .SingleOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ValidationException(nameof(request.ProductId), "Product was not found");
        }

        product.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        using var scope = new MapContextScope();
        scope.Context.Parameters[nameof(IImageStorageService)] = imageStorage;

        return mapper.Map<ProductDto>(product);
    }
}
