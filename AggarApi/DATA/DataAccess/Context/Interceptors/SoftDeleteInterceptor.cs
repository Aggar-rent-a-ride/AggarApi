using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DATA.Models.Contract;

namespace DATA.DataAccess.Context.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
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
            }

            return result;
        }
    }
}
