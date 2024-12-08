namespace AggarApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public double Behavior { get; set; }
        public double Punctuality { get; set; }
        public string? Comments { get; set; }

        public Notification? Notification { get; set; }
        public Rental Rental { get; set; } = null!;
        public ICollection<Report>? Reports { get; set; }
    }
}
