using Handmade.Application.Features.Users.Models;
using Handmade.Application.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(
    int UserId
) : IRequest<UserDto?>;

public sealed class GetUserByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper): IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(
        GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .ProjectToType<UserDto>(mapper.Config)
            .FirstAsync(x => x.Id == request.UserId ,cancellationToken);
    }
}