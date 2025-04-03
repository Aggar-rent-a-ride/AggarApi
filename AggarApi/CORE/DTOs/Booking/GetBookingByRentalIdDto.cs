using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Booking
{
    public class GetBookingByRentalIdDto
    {
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Discount { get; set; }
        public BookingStatus Status { get; set; }
    }
}
