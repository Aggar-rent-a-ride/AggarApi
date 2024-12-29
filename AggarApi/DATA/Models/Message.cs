namespace DATA.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public string? AttachmentPath { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool Seen { get; set; } = false;

        public AppUser Sender { get; set; } = null!;
        public AppUser Receiver { get; set; } = null!;
        public Notification? Notification { get; set; }
        public Report? Report { get; set; }
    }
}
