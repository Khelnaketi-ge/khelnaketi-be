using Handmade.Application.Features.Users.Models;
using Handmade.Application.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<List<UserDto>>;

public sealed class GetUsersQueryHandler(
    IApplicationDbContext context,
    IMapper mapper): IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .ProjectToType<UserDto>(mapper.Config)
            .ToListAsync(cancellationToken);
    }
}