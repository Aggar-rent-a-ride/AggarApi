using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Booking
{
    public class BookingDetailsDto : CreateBookingDto
    {
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int TotalDays { get; set; }
        public decimal Discount { get; set; }
        public string? Notes { get; set; }
        public BookingStatus Status { get; set; }
    }
}
