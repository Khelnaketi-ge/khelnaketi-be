namespace Handmade.Domain.Common;

public interface IBaseAuditableEntity
{
    DateTimeOffset Created { get; set; }
    int? CreatedBy { get; set; }
    DateTimeOffset Updated { get; set; }
    int? UpdatedBy { get; set; }
    bool Deleted { get; set; }
}