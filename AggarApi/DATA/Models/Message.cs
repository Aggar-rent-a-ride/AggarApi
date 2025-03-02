using System.ComponentModel.DataAnnotations.Schema;

namespace DATA.Models
{
    public class Message
    {
        public int Id { get; set; }
        [NotMapped]
        public string ClientMessageId { get; set; } = null!;
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsSeen { get; set; } = false;
        public string MessageType { get; set; }

        public AppUser Sender { get; set; } = null!;
        public AppUser Receiver { get; set; } = null!;
        public Notification? Notification { get; set; }
        public Report? Report { get; set; }
        
    }
}
