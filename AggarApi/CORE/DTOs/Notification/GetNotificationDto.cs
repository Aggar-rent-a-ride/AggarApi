using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATA.Models.Enums;

namespace CORE.DTOs.Notification
{
    public class GetNotificationDto
    {
        public int Id { get; set; }
        public DateTime SentAt { get; set; }
        public string Content { get; set; }
        public bool IsSeen { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TargetType TargetType { get; set; }
        public int? TargetBookingId { get; set; }
        public int? TargetRentalId { get; set; }
        public int? TargetCustomerReviewId { get; set; }
        public int? TargetRenterReviewId { get; set; }
        public int? TargetMessageId { get; set; }
    }
}
