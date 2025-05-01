using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using DATA.Models.Contract;

namespace DATA.Models
{
    public class AppUser : IdentityUser<int>, ISoftDeleteable
    {
        public string Name { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        [NotMapped]
        public int Age
        {
            get => DateTime.Today.Year - DateOfBirth.Year;
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool AggreedTheTerms { get; set; }
        public double? Rate { get; set; }
        public Enums.UserStatus Status { get; set; } = Enums.UserStatus.Inactive;
        public int WarningCount { get; set; } = 0;
        public DateTime? ActivateIn { get; set; }
        public string? ImagePath { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public Location Location { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }

        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<Message>? Messages { get; set; }
        public ICollection<Message>? ReceivedMessages { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<Report>? TargetReports { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
