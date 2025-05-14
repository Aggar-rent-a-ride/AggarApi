using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA.Models.Enums;

namespace CORE.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public TargetType TargetType { get; set; }
        public int TargetId { get; set; }
    }
}
