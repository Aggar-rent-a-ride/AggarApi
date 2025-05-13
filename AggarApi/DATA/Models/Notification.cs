using DATA.Models.Enums;

namespace DATA.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string Content { get; set; } = null!;
        public bool IsSeen { get; set; } = false;
        public AppUser Reciver { get; set; } = null!;

        public TargetType TargetType { get; set; }
        public int? TargetBookingId { get; set; }
        public int? TargetRentalId { get; set; }
        public int? TargetCustomerReviewId { get; set; }
        public int? TargetRenterReviewId { get; set; }
        public int? TargetMessageId { get; set; }
        public Rental? TargetRental { get; set; }
        public Booking? TargetBooking { get; set; }
        public CustomerReview? TargetCustomerReview { get; set; }
        public RenterReview? TargetRenterReview { get; set; }
        public Message? TargetMessage { get; set; }
    }
}
