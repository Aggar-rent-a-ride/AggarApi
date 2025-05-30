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
    }
}
