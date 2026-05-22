using Handmade.Application.Interfaces;
using Handmade.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Handmade.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<IBaseAuditableEntity>())
        {
            var hasChangedOwnedEntities = entry.HasChangedOwnedEntities();

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)
                && !hasChangedOwnedEntities) continue;
            
            var utcNow = timeProvider.GetUtcNow();
            
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = utcNow;
                    entry.Entity.CreatedBy = currentUser.Id;
                    break;
                case EntityState.Modified:
                    entry.Entity.Updated = utcNow;
                    entry.Entity.UpdatedBy = currentUser.Id;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.Deleted = true;
                    entry.Entity.Updated = utcNow;
                    entry.Entity.UpdatedBy = currentUser.Id;
                    break;
                default:
                    if (hasChangedOwnedEntities)
                    {
                        entry.Entity.Updated = utcNow;
                        entry.Entity.UpdatedBy = currentUser.Id;
                    }

                    break;
            }
        }
    }
}

internal static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            r.TargetEntry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
}
