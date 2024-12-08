using AggarApi.Models.Enums;

namespace AggarApi.Models
{
    public class AdminAction
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public int TargetUserId { get; set; }
        public Enums.AdminActionType Type { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime DueTo { get; set; }
        public Admin Admin { get; set; } = null!;
        public AppUser TargetUser { get; set; } = null!;
        public Notification? Notification { get; set; }
    }
}
