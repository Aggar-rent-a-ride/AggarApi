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
        public long Progress { get; set; } //number of bytes uploaded, flutter can use it to calculate progress in percentage
    }
}
