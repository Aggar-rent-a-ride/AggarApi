namespace AggarApi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Price { get; set; }
        public Enums.BookingStatus Status { get; set; }

        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public Notification? Notification { get; set; }
        public Rental? Rental { get; set; }
    }
}
