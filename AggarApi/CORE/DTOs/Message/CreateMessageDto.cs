using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CORE.DTOs.Message
{
    public class CreateMessageDto
    {
        public string ClientMessageId { get; set; } = null!;
        public int ReceiverId { get; set; }
        public string Content { get; set; } = null!;
    }
}
