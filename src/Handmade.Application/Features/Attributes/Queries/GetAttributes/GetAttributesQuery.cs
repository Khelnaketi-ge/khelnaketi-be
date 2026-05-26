using Handmade.Application.Features.Attributes.Models;
using Handmade.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Handmade.Application.Features.Attributes.Queries.GetAttributes;

public sealed record GetAttributesQuery : IRequest<IReadOnlyCollection<AttributeDto>>;

public sealed class GetAttributesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAttributesQuery, IReadOnlyCollection<AttributeDto>>
{
    public async Task<IReadOnlyCollection<AttributeDto>> Handle(
        GetAttributesQuery request,
        CancellationToken cancellationToken)
    {
        var attributes = await context.ProductAttributes
            .AsNoTracking()
            .Include(x => x.Options)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return attributes.Select(AttributeMappings.ToDto).ToList();
    }
}
