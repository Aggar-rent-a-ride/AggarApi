using AggarApi.Models.Enums;

namespace AggarApi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ReceiverId { get; set; }
        public int? TargetId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string Content { get; set; } = null!;
        public bool Seen { get; set; } = false;

        public AppUser Reciver { get; set; } = null!;

        public TargetType? Target { get; set; }
        public Booking? TargetBooking { get; set; }
        public Message? TargetMessage { get; set; }
        public AdminAction? TargetAdminAction { get; set; }
        public RenterReview? TargetRenterReview { get; set; }
        public CustomerReview? TargetCustomerReview { get; set; }
    }
}
