using CORE.DTOs.Booking;
using CORE.DTOs.Vehicle;
using DATA.Models;
using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Rental
{
    public class GetRentalDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerReviewId { get; set; }
        public int RenterReviewId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RentalStatus RentalStatus { get; set; }
        public GetBookingByRentalIdDto Booking { get; set; }
        public GetVehicleDto Vehicle { get; set; }
    }
}
