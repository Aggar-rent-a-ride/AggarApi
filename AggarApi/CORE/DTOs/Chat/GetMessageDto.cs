using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Message
{
    public class GetMessageDto
    {
        public int Id { get; set; }
        public string ClientMessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsSeen { get; set; } 
    }
}
