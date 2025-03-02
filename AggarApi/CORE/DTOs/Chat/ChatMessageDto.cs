namespace CORE.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public DateTime SentAt { get; set; }
        public bool Seen { get; set; }
    }
}
