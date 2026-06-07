using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Cart.Models;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(int ProductId) : IRequest<CartStateDto>;

public sealed class AddCartItemCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser) : IRequestHandler<AddCartItemCommand, CartStateDto>
{
    public async Task<CartStateDto> Handle(
        AddCartItemCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.Id is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.InvalidCreds);
        }

        var productExists = await context.Products
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.ProductId
                    && x.Status == ProductStatus.Active
                    && x.Brand.Status == BrandStatus.Active,
                cancellationToken);

        if (!productExists)
        {
            return new CartStateDto(request.ProductId, false);
        }

        var activeCart = await context.Carts
            .FirstOrDefaultAsync(
                x => x.UserId == currentUser.Id.Value && x.Status == CartStatus.Active,
                cancellationToken);

        if (activeCart is null)
        {
            activeCart = new Domain.Entities.Cart
            {
                UserId = currentUser.Id.Value,
                Status = CartStatus.Active
            };

            context.Carts.Add(activeCart);
            await context.SaveChangesAsync(cancellationToken);
        }

        var cartItem = await context.CartItems
            .FirstOrDefaultAsync(
                x => x.CartId == activeCart.Id && x.ProductId == request.ProductId,
                cancellationToken);

        if (cartItem is not null)
        {
            cartItem.Quantity += 1;
            await context.SaveChangesAsync(cancellationToken);
            return new CartStateDto(request.ProductId, true);
        }

        context.CartItems.Add(new CartItem
        {
            CartId = activeCart.Id,
            ProductId = request.ProductId,
            Quantity = 1
        });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return new CartStateDto(request.ProductId, true);
        }

        return new CartStateDto(request.ProductId, true);
    }

    private static bool IsUniqueConstraintViolation(Exception exception) =>
        GetSqlState(exception) == "23505"
        || exception.InnerException is not null && IsUniqueConstraintViolation(exception.InnerException);

    private static string? GetSqlState(Exception exception)
    {
        var property = exception.GetType().GetProperty("SqlState");
        return property?.GetValue(exception) as string;
    }
}
