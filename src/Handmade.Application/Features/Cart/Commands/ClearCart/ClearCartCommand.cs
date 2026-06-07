using Handmade.Application.Common.Exceptions;
using Handmade.Application.Interfaces;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Commands.ClearCart;

public sealed record ClearCartCommand : IRequest<int>;

public sealed class ClearCartCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<ClearCartCommand, int>
{
    public async Task<int> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var cartItems = await context.CartItems
            .Where(x =>
                x.Cart.UserId == currentUser.Id.Value
                && x.Cart.Status == CartStatus.Active)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return 0;
        }

        context.CartItems.RemoveRange(cartItems);
        await context.SaveChangesAsync(cancellationToken);

        return cartItems.Count;
    }
}
