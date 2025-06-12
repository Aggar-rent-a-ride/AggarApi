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
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Booking?> GetBookingByRentalIdAsync(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Booking)
                .ThenInclude(b => b.Vehicle)
                .ThenInclude(v => v.VehicleBrand)
                .Include(r => r.Booking)
                .ThenInclude(b => b.Vehicle)
                .ThenInclude(v => v.Renter)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            return rental?.Booking;
        }
        public async Task<Booking?> GetBookingByIntentIdAsync(string intentId)
        {
            Booking? booking = await _context.bookings
                .Where(b => b.PaymentIntentId == intentId)
                .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleBrand)
                .Include(b => b.Vehicle)
                .ThenInclude(v => v.Renter)
                .FirstOrDefaultAsync();

            return booking;
        }

        public async Task<IEnumerable<Interval>> GetBookingsInterval(int rentalId)
        {
            var intervals = await _context.bookings
                .Include(b => b.Vehicle)
                .Where(b => b.Vehicle.RenterId == rentalId && (b.Status == Models.Enums.BookingStatus.Confirmed || b.Status == Models.Enums.BookingStatus.Accepted))
                .Select(b => new Interval
                {
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                })
                .OrderBy(b => b.StartDate)
                .ToListAsync();

            return intervals;
        }
    }
}
