using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class Brand : BaseAuditableEntity<int>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; } 
    
    public string? LegalName { get; set; }
    public Guid? LogoImageId { get; set; }
    public ImageAsset? LogoImage { get; set; }
    
    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;
    
    public BrandStatus Status { get; set; } = BrandStatus.Active;

    public ICollection<BrandRole> Roles { get; set; } = [];
    public ICollection<BrandMember> Members { get; set; } = [];
    public ICollection<BrandInvitation> Invitations { get; set; } = [];
    public ICollection<BrandAddress> Addresses { get; set; } = [];
    public ICollection<BrandEmailAddress> EmailAddresses { get; set; } = [];
    public ICollection<BrandPhoneNumber> PhoneNumbers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
