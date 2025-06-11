using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Booking
{
    public class BookingSummaryDto
    {
        public int Id { get; set; }
        public string VehicleBrand { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal FinalPrice { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingStatus BookingStatus { get; set; }
    }
}
