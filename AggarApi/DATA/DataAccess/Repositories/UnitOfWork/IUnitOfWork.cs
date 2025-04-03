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
        IBaseRepository<Renter> Renters { get; }
        IVehicleRepository Vehicles { get; }
        IBaseRepository<Discount> Discounts { get; }
        IBookingRepository Bookings { get; }
        IChatRepository Chat { get; }
        IBaseRepository<FileCache> FileCache { get; }
        IBaseRepository<VehicleBrand> VehicleBrands { get; }
        IBaseRepository<VehicleType> VehicleTypes { get; }
        IRentalRepository Rentals { get; }
        IBaseRepository<RenterReview> RenterReviews { get; }
        IBaseRepository<CustomerReview> CustomerReviews { get; }

        Task<int> CommitAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
