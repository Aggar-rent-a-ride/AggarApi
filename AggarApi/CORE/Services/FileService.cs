using CORE.Services.IServices;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using CORE.DTOs.Paths;
using DATA.Constants;

namespace CORE.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<string?> UploadFileAsync(string dir, string oldFilePath, IFormFile file, List<string> allowedExtensions = null)
        {
            if (file == null)
                return null;

            try
            {
                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (allowedExtensions == null || allowedExtensions.Contains(fileExtension) == false)
                    throw new InvalidOperationException("Invalid file type.");

                // Generate unique file name
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{fileExtension}";

                // Ensure directory exists
                var uploadDir = Path.Combine(_environment.WebRootPath, dir);
                Directory.CreateDirectory(uploadDir);

                // File paths
                var newFilePath = Path.Combine(uploadDir, uniqueFileName);
                var publicPath = $"/{dir}/{uniqueFileName}";

                // Save the new file
                using (var stream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }

                // Delete the old file, if it exists
                if (string.IsNullOrEmpty(oldFilePath) == false)
                {
                    var oldFileFullPath = Path.Combine(_environment.WebRootPath, oldFilePath.TrimStart('/'));
                    if (File.Exists(oldFileFullPath) == true)
                        File.Delete(oldFileFullPath);
                }
                return publicPath;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                var completeFilePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(completeFilePath) == false)
                    return false;

                File.Delete(completeFilePath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
