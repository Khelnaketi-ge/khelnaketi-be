using Handmade.Application.Common.Exceptions;
using Handmade.Application.Features.Users.Models;
using Handmade.Application.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Users.Commands.EditUser;

public sealed record EditUserCommand(
    int Id,
    string FirstName,
    string LastName,
    bool EmailVerified,
    bool PhoneNumberVerified,
    bool IsBlocked) : IRequest<UserDto>;
    
public sealed class EditUserCommandHandler(
    IApplicationDbContext context,
    IMapper mapper): IRequestHandler<EditUserCommand, UserDto>
{
    public async Task<UserDto> Handle(EditUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(x => x.OwnedBrands)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException(UnauthorizedErrors.UserNotFound);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        user.EmailVerified = request.EmailVerified;
        user.PhoneNumberVerified = request.PhoneNumberVerified;
        user.IsBlocked = request.IsBlocked;

        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserDto>(user);
    }
}