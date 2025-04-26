using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Constants
{
    public static class AllowedExtensions
    {
        public static List<string> ImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".heic", ".webp", ".svg" };
        public static List<string> FileExtensions = ImageExtensions.Concat(new List<string> { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" }).ToList();
    }
}
