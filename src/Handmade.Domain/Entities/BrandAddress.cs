using Handmade.Domain.Common;

namespace Handmade.Domain.Entities;

public class BrandAddress : BaseAuditableEntity<int>
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    
    public required string City { get; set; }
    
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    
    public string? PostalCode { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public bool IsPrimary { get; set; }
    
    public bool IsActive { get; set; } = true;
}