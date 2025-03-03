using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Chat
{
    public class MessageFilterDto
    {
        public int UserId { get; set; }
        public string? SearchQuery { get; set; }
        public DateTime? Date { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }
    }
}
