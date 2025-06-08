using CORE.DTOs.Review;
using DATA.Constants.Enums;
using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Rental
{
    public class RentalHistoryItemDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RentalStatus RentalStatus { get; set; }
        public ReviewDetails? RenterReview { get; set; }
        public ReviewDetails? CustomerReview { get; set; }
        public VehicleDetails Vehicle { get; set; }
        public UserDetails User { get; set; }
        public class VehicleDetails
        {
            public int Id { get; set; }
            public decimal PricePerDay { get; set; }
            public string MainImagePath { get; set; }
            public string? Address { get; set; }
        }
        public class UserDetails
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? ImagePath { get; set; }
        }
        public class ReviewDetails : GetReviewDto
        {
            public UserDetails Reviewer { get; set; }
        }
    }
}
