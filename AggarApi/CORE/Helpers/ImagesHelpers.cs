using Microsoft.AspNetCore.Http;

namespace CORE.Helpers
{
    public static class ImagesHelpers
    {

        public static IFormFile? ConvertToIFormFile(string imagePath)
        {

            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

            if (!File.Exists(absolutePath))
                return null;

            var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, "MainImage", Path.GetFileName(imagePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = GetMimeType(absolutePath)
            };
        }

        private static string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream",
            };
        }
    }
}
