using CORE.DTOs.Booking;
using CORE.DTOs.Vehicle;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Rental
{
    public class GetRentalDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerReviewId { get; set; }
        public int RenterReviewId { get; set; }
        public GetBookingByRentalIdDto Booking { get; set; }
        public GetVehicleDto Vehicle { get; set; }
    }
}
