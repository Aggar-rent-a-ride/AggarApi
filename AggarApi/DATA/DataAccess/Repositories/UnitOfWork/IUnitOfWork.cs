using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<AppUser> AppUsers { get; }
        IVehicleRepository Vehicles { get; }
        IBaseRepository<Discount> Discounts { get; }
        Task<int> CommitAsync();
    }
}
