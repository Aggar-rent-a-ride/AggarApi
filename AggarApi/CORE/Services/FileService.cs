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
using System.Security.Cryptography;
using Microsoft.AspNetCore.Razor.Hosting;

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
                DeleteFile(oldFilePath);
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
        public string? HashFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            var completeFilePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(completeFilePath) == false)
                return null;

            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = File.ReadAllBytes(completeFilePath);
                var checksum = Convert.ToBase64String(sha256.ComputeHash(bytes));
                return checksum;
            }
        }
        public async Task<string?> CreateFile(string dir, string fileName, string fileExtension, List<string> allowedExtensions)
        {
            if(dir == null || fileName == null || fileExtension == null || allowedExtensions == null)
                return null;

            if (allowedExtensions.Contains(fileExtension) == false)
                return null;

            // Generate unique file name
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{fileExtension}";

            // Ensure directory exists
            var uploadDir = Path.Combine(_environment.WebRootPath, dir);
            Directory.CreateDirectory(uploadDir);

            // File paths
            var newFilePath = Path.Combine(uploadDir, uniqueFileName);
            var publicPath = $"/{dir}/{uniqueFileName}";

            // Save the new file
            File.Create(newFilePath).Close();

            return publicPath;
        }
    }
}
