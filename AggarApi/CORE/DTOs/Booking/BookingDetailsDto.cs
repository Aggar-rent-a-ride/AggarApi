using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Booking
{
    public class BookingDetailsDto : CreateBookingDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerImagePath { get; set; }
        public int TotalDays { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Discount { get; set; }
        public string VehicleImagePath { get; set; } = null!;
        public int VehicleYear { get; set; }
        public string? VehicleBrand { get; set; }
        public string? VehicleType { get; set; }
        public string? VehicleModel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingStatus Status { get; set; }
    }
}
