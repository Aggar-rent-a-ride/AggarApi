using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(string dir, string oldFilePath, IFormFile file, List<string> allowedExtensions = null);
        Task<string?> CreateFile(string dir, string fileName, string fileExtension, List<string> allowedExtensions);
        string? HashFile(string filePath);
        bool DeleteFile(string filePath);
    }
}
