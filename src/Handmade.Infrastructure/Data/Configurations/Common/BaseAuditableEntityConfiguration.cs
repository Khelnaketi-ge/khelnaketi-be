using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Handmade.Infrastructure.Data.Configurations.Common;

internal abstract class BaseAuditableEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : class, IBaseAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Created)
            .IsRequired();

        builder.Property(x => x.Updated)
            .IsRequired();

        builder.Property(x => x.Deleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Deleted)
            .HasFilter("\"Deleted\" = false");
        
        builder.HasQueryFilter(x => !x.Deleted);
    }
}
