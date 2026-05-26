using Handmade.Domain.Common;
using Handmade.Domain.Enums;

namespace Handmade.Domain.Entities;

public class Brand : BaseAuditableEntity<int>, INormalizedNameEntity
{
    public required string Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    
    public string? LegalName { get; set; }
    public Guid? LogoImageId { get; set; }
    public ImageAsset? LogoImage { get; set; }
    
    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;
    
    public BrandStatus Status { get; set; } = BrandStatus.Active;

    public ICollection<BrandAddress> Addresses { get; set; } = [];
    public ICollection<BrandEmailAddress> EmailAddresses { get; set; } = [];
    public ICollection<BrandPhoneNumber> PhoneNumbers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
