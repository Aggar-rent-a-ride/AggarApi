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

        public async Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize)
        {
            return await _context.Rentals
                .Where(r => r.Booking.CustomerId == userId || r.Booking.Vehicle.RenterId == userId)
                .Select(r=> new Rental
                {
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    BookingId = r.BookingId,
                    Booking = new Booking
                    {
                        Id = r.Booking.Id,
                        CustomerId = r.Booking.CustomerId,
                        Vehicle = new Vehicle
                        {
                            Id = r.Booking.Vehicle.Id,
                            RenterId = r.Booking.Vehicle.RenterId
                        }
                    }
                })
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
