using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Message
{
    public class FileUploadProgressDto
    {
        public string ClientMessageId { get; set; }
        public int Progress { get; set; }
    }
}
