using Handmade.Domain.Enums;

namespace Handmade.Application.Features.Attributes.Models;

public sealed record AttributeDto(
    int Id,
    string Name,
    AttributeType Type,
    string? Unit,
    bool IsDisabled,
    IReadOnlyCollection<AttributeOptionDto> Options);

public sealed record AttributeOptionDto(
    int Id,
    string Value,
    int Order);
