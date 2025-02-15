using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Message
{
    public class FileInfoRequestDto
    {
        public string ClientMessageId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
}
