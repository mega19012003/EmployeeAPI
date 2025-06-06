using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace EmployeeAPI.Services.ImageServices
{
    public interface ICloudImageService
    {
        Task<string> UploadImageAsync(IFormFile file);

        Task<bool> DeleteImageAsync(string publicId);

        string? ExtractPublicId(string imageUrl);
    }
}
