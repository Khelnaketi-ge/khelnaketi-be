using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Cart.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Queries.GetCartSummary;

public sealed record GetCartSummaryQuery : IRequest<CartSummaryDto>;

public sealed class GetCartSummaryQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<GetCartSummaryQuery, CartSummaryDto>
{
    public async Task<CartSummaryDto> Handle(
        GetCartSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var items = await context.CartItems
            .AsNoTracking()
            .Where(x =>
                x.Cart.UserId == currentUser.Id.Value
                && x.Cart.Status == CartStatus.Active
                && x.Product.Status == ProductStatus.Active
                && x.Product.Brand.Status == BrandStatus.Active)
            .Select(x => new
            {
                x.ProductId,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        return new CartSummaryDto(
            items.Sum(x => x.Quantity),
            items.Select(x => x.ProductId).ToList());
    }
}
