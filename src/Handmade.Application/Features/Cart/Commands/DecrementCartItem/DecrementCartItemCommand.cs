using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Cart.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Commands.DecrementCartItem;

public sealed record DecrementCartItemCommand(int ProductId) : IRequest<CartStateDto>;

public sealed class DecrementCartItemCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<DecrementCartItemCommand, CartStateDto>
{
    public async Task<CartStateDto> Handle(
        DecrementCartItemCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var cartItem = await context.CartItems
            .FirstOrDefaultAsync(
                x => x.Cart.UserId == currentUser.Id.Value
                    && x.Cart.Status == CartStatus.Active
                    && x.ProductId == request.ProductId,
                cancellationToken);

        if (cartItem is null)
        {
            return new CartStateDto(request.ProductId, false);
        }

        if (cartItem.Quantity <= 1)
        {
            context.CartItems.Remove(cartItem);
            await context.SaveChangesAsync(cancellationToken);
            return new CartStateDto(request.ProductId, false);
        }

        cartItem.Quantity -= 1;
        await context.SaveChangesAsync(cancellationToken);

        return new CartStateDto(request.ProductId, true);
    }
}
