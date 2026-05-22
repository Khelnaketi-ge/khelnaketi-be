using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class BrandMember : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int RoleId { get; set; }
    public BrandRole Role { get; set; } = null!;
}