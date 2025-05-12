using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IRentalRepository : IBaseRepository<Rental>
    {
        Task<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?> GetRentalByIdIncludingBookingThenIncludingVehicleAsync(int rentalId);
        Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize);
        Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId);
        Task<IEnumerable<Rental>> GetRentalsByVehicleIdAsync(int vehicleId, int pageNo, int pageSize);
        Task<IEnumerable<Rental>> GetUserRentalHistoryAsync(int userId, int pageNo, int pageSize);
        Task<int> GetUserRentalHistoryCountAsync(int userId);
        Task<int> GetRentalsByUserIdCountAsync(int userId, string role);
    }
}
