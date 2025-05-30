namespace DATA.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerReviewId { get; set; }
        public int RenterReviewId { get; set; }
        public string hashedQrToken { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public CustomerReview? CustomerReview { get; set; }
        public RenterReview? RenterReview { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
