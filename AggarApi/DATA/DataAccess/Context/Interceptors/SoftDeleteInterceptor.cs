using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DATA.Models.Contract;

namespace DATA.DataAccess.Context.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
                return result;

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry is { State: EntityState.Deleted, Entity: ISoftDeleteable entity })
                {
                    entry.State = EntityState.Modified;

                    entity.Delete();

                    /*foreach (var property in entry.Properties)
                    {
                        string propertyName = property.Metadata.Name;

                        if (propertyName != "IsDeleted" && propertyName != "DateDeleted")
                        {
                            property.IsModified = false;
                        }
                    }*/
                }
            }

            return result;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is null)
                return result;

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry is not { State: EntityState.Deleted, Entity: ISoftDeleteable entity })
                    continue;

                entry.State = EntityState.Modified;

                entity.Delete();

                /*foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    if (propertyName != "IsDeleted" && propertyName != "DateDeleted")
                    {
                        property.IsModified = false;
                    }
                }*/
            }

            return result;
        }
    }
}
