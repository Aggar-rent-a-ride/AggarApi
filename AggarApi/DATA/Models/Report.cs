namespace DATA.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public int? TargetId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Enums.ReportStatus Status { get; set; } = Enums.ReportStatus.Pending;
        public AppUser Reporter { get; set; } = null!;

        public Enums.TargetType? TargetType { get; set; }
        public Message? TargetMessage { get; set; }
        public RenterReview? TargetRenterReview { get; set; }
        public CustomerReview? TargetCustomerReview { get; set; }
        public AppUser? TargetUser { get; set; }
        public Vehicle? TargetVehicle { get; set; }
    }
}
