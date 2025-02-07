using System.ComponentModel.DataAnnotations.Schema;

namespace DATA.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [NotMapped]
        public int TotalDays
        {
            get
            {
                int totalDays = (EndDate - StartDate).Days;
                if ((EndDate - StartDate).TotalHours > totalDays * 24)
                    totalDays++;

                return totalDays;
            }
        }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        [NotMapped]
        public decimal FinalPrice
        {
            get
            {
                return Price - Price * (Discount / 100);
            }
        }
        public Enums.BookingStatus Status { get; set; } = Enums.BookingStatus.Pending;

        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Notification? Notification { get; set; }
        public Rental? Rental { get; set; }
    }
}
