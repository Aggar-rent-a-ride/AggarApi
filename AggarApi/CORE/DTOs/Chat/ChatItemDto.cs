using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Chat
{
    public class ChatItemDto<T>
    {
        public IEnumerable<int> UnseenMessageIds { get; set; } = new List<int>();
        public ChatUserDto User { get; set; }
        public T LastMessage { get; set; }
    }
}
