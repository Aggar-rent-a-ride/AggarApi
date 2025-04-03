using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories
{
    public class RentalRepository : BaseRepository<Rental>, IRentalRepository
    {
        public RentalRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId)?> GetRentalByIdIncludingBookingThenIncludingVehicleAsync(int rentalId)
        {
            var result = await _context.Rentals
                .Select(r=> new { 
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    CustomerId = r.Booking.CustomerId,
                    RenterId = r.Booking.Vehicle.RenterId,
                })
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            return result == null
                ? default
                : (result.Id, result.CustomerReviewId, result.RenterReviewId,
                   result.CustomerId, result.RenterId);
        }
    }
}
