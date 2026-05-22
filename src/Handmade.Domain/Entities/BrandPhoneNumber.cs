using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class BrandPhoneNumber : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public required string PhoneNumber { get; set; }
    public required string NormalizedPhoneNumber { get; set; }

    public string? Label { get; set; }

    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}