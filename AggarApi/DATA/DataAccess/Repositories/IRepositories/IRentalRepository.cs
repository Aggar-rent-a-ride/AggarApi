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
        Task<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?> GetRentalByIdIncludingBookingThenIncludingVehicleAsync(int rentalId);
        Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize);
    }
}
