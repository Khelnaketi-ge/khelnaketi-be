namespace Handmade.Domain.Common;

public class BaseAuditableEntity<T> : BaseEntity<T>, IBaseAuditableEntity
{
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public int? UpdatedBy { get; set; }
    public bool Deleted { get; set; }
}