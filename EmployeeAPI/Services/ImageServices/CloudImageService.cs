using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace EmployeeAPI.Services.ImageServices
{
    public class CloudImageService : ICloudImageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudImageService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            const int maxFileSize = 10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new ArgumentException("Ảnh quá lớn, hãy chọn ảnh có kích thước >= 10 MB");
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "UserProfilePicture",

                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Gravity("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            throw new Exception("Upload image failed");
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            Console.WriteLine($"[Xoá ảnh] Public ID: {publicId} - Kết quả: {deletionResult?.Result}");

            return deletionResult.Result == "ok" || deletionResult.Result == "not found";
        }

        public string? ExtractPublicId(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;
            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;

                var match = Regex.Match(path, @"/upload/(?:v\d+/)?(.+)\.\w+$");
                if (match.Success)
                {
                    return match.Groups[1].Value; 
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid image URL format", ex);
            }

            return null;
        }
    }
}