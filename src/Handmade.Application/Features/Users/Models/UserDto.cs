using Handmade.Domain.Entities;
using Handmade.Domain.Enums;
using Mapster;

namespace Handmade.Application.Features.Users.Models;

public class UserDto : IMapFrom<User>
{
    public int Id { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public required string Email { get; set; }
    public bool EmailVerified { get; set; }

    public string? PhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; }

    public bool IsBlocked { get; set; }
    public AccessLevel AccessLevel { get; set; }

    public List<UserOwnedBrandDto> OwnedBrands { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.OwnedBrands, src => src.OwnedBrands);
    }
}

public sealed class UserOwnedBrandDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}