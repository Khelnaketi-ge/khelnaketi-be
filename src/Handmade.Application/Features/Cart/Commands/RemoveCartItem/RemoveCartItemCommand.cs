using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Cart.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(int ProductId) : IRequest<CartStateDto>;

public sealed class RemoveCartItemCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<RemoveCartItemCommand, CartStateDto>
{
    public async Task<CartStateDto> Handle(
        RemoveCartItemCommand request,
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

        if (cartItem is not null)
        {
            context.CartItems.Remove(cartItem);
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CartStateDto(request.ProductId, false);
    }
}
