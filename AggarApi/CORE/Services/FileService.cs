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
using CORE.DTOs;
using CORE.DTOs.Message;
using CORE.Constants;
using StatusCodes = CORE.Constants.StatusCodes;
using Microsoft.Extensions.Logging;

namespace CORE.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }
        public async Task<string?> UploadFileAsync(string dir, string oldFilePath, IFormFile file, List<string> allowedExtensions = null)
        {
            if (file == null)
            {
                _logger.LogWarning("UploadFileAsync: No file was provided for upload.");
                return null;
            }

            try
            {
                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (allowedExtensions == null || allowedExtensions.Contains(fileExtension) == false)
                {
                    _logger.LogWarning("UploadFileAsync: Invalid file type '{FileExtension}' for file '{FileName}'", fileExtension, file.FileName);
                    throw new InvalidOperationException("Invalid file type.");
                }

                // Generate unique file name
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{fileExtension}";

                // Ensure directory exists
                var uploadDir = Path.Combine(_environment.WebRootPath, dir);
                Directory.CreateDirectory(uploadDir);

                // File paths
                var newFilePath = Path.Combine(uploadDir, uniqueFileName);
                var publicPath = $"/{dir}/{uniqueFileName}";

                _logger.LogInformation("Uploading file '{FileName}' to '{NewFilePath}'", file.FileName, newFilePath);

                // Save the new file
                using (var stream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File '{FileName}' uploaded successfully to '{PublicPath}'", file.FileName, publicPath);

                _logger.LogInformation("Deleting old file '{OldFilePath}'", oldFilePath);
                // Delete the old file, if it exists
                DeleteFile(oldFilePath);
                return publicPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadFileAsync: Failed to upload file '{FileName}'", file.FileName);
                return null;
            }
        }
        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("DeleteFile: Provided file path is null or empty.");
                return false;
            }
            try
            {
                var completeFilePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(completeFilePath) == false)
                {
                    _logger.LogWarning("DeleteFile: File '{FilePath}' does not exist.", completeFilePath);
                    return false;
                }

                File.Delete(completeFilePath);
                _logger.LogInformation("DeleteFile: Successfully deleted file '{FilePath}'.", completeFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteFile: Failed to delete file '{FilePath}'.", filePath);
                return false;
            }
        }
        public string? HashFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("HashFile: Provided file path is null or empty.");
                return null;
            }

            var completeFilePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(completeFilePath) == false)
            {
                _logger.LogWarning("HashFile: File '{FilePath}' does not exist.", completeFilePath);
                return null;
            }

            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    var bytes = File.ReadAllBytes(completeFilePath);
                    var checksum = Convert.ToBase64String(sha256.ComputeHash(bytes));

                    _logger.LogInformation("HashFile: Successfully generated hash for file '{FilePath}'.", completeFilePath);
                    return checksum;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HashFile: Failed to hash file '{FilePath}'.", completeFilePath);
                return null;
            }
        }
        public async Task<string?> CreateFile(string dir, string fileName, string fileExtension, List<string> allowedExtensions)
        {
            if(dir == null || fileName == null || fileExtension == null || allowedExtensions == null)
            {
                _logger.LogWarning("CreateFile: One or more required parameters are null.");
                return null;
            }

            if (allowedExtensions.Contains(fileExtension) == false)
            {
                _logger.LogWarning("CreateFile: File extension '{FileExtension}' is not allowed.", fileExtension);
                return null;
            }

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

            _logger.LogInformation("CreateFile: Successfully created file at '{FilePath}'.", newFilePath);
            return publicPath;
        }
        public async Task<ResponseDto<FileUploadProgressDto>> UploadFileAsync(string filePath, string bytesBase64, long maximumFileBytes)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(bytesBase64))
            {
                _logger.LogWarning("UploadFileAsync: Invalid file path or base64 data.");
                return new ResponseDto<FileUploadProgressDto> { Data = new FileUploadProgressDto { Progress = 0 }, StatusCode = StatusCodes.BadRequest, Message = "Invalid file path or bytes" };
            }

            var bytes = Convert.FromBase64String(bytesBase64);
            try
            {
                var completeFilePath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(completeFilePath) == false)
                {
                    _logger.LogWarning("UploadFileAsync: File '{FilePath}' does not exist.", completeFilePath);
                    return new ResponseDto<FileUploadProgressDto> { Data = new FileUploadProgressDto { Progress = 0 }, StatusCode = StatusCodes.BadRequest, Message = "File doesn't exist" };
                }

                await using (var stream = new FileStream(completeFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true))
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }

                var fileLength = new FileInfo(completeFilePath).Length;
                if(fileLength > maximumFileBytes)
                {
                    _logger.LogWarning("UploadFileAsync: File '{FilePath}' exceeded the maximum size limit of {MaxSize} bytes.", completeFilePath, maximumFileBytes);
                    DeleteFile(filePath);
                    return new ResponseDto<FileUploadProgressDto> { Data = new FileUploadProgressDto { Progress = 0 }, StatusCode = StatusCodes.BadRequest, Message = "File size exceeded" };
                }

                _logger.LogInformation("UploadFileAsync: Successfully uploaded file '{FilePath}'. Current size: {FileSize} bytes.", completeFilePath, fileLength);
                return new ResponseDto<FileUploadProgressDto> { Data = new FileUploadProgressDto { Progress = fileLength }, StatusCode = StatusCodes.OK };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadFileAsync: Error while uploading file '{FilePath}'.", filePath);
                return new ResponseDto<FileUploadProgressDto> { Data = new FileUploadProgressDto { Progress = 0 }, StatusCode = StatusCodes.InternalServerError, Message = ex.Message };
            }
        }
    }
}
