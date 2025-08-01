﻿using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        Task<Booking?> GetBookingByRentalIdAsync(int rentalId);
        Task<Booking?> GetBookingByIntentIdAsync(string intentId);
        Task<IEnumerable<Interval>> GetBookingsInterval(int rentalId);
    }
}
