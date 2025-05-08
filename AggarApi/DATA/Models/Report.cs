using DATA.Models.Enums;

namespace DATA.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Enums.ReportStatus Status { get; set; } = Enums.ReportStatus.Pending;
        public AppUser Reporter { get; set; } = null!;

        public TargetType TargetType { get; set; }
        public int? TargetAppUserId { get; set; }
        public int? TargetVehicleId { get; set; }
        public int? TargetCustomerReviewId { get; set; }
        public int? TargetRenterReviewId { get; set; }
        public int? TargetMessageId { get; set; }
        public AppUser? TargetAppUser { get; set; }
        public Vehicle? TargetVehicle { get; set; }
        public CustomerReview? TargetCustomerReview { get; set; }
        public RenterReview? TargetRenterReview { get; set; }
        public Message? TargetMessage { get; set; }
    }
}
