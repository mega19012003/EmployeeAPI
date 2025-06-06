using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmployeeAPI.Services.FileServices
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> SaveFileAsync(IFormFile file, string? subFolder = "images")
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File null or empty");
                return null;
            }

            try
            {
                // Lấy đường dẫn tuyệt đối đến folder wwwroot/images hoặc folder con
                var uploadsFolder = Path.Combine(_env.WebRootPath, subFolder ?? "images");
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation("Uploads folder: {UploadsFolder}", uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine(subFolder ?? "images", uniqueFileName).Replace("\\", "/");
                _logger.LogInformation("File saved successfully: {RelativePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file");
                throw;
            }
        }

        public async Task<string?> UpdateFileAsync(IFormFile newFile, string? subFolder = "images", string? oldFilePath = null)
        {
            if (newFile == null || newFile.Length == 0)
            {
                _logger.LogWarning("New file is null or empty, returning old file path");
                return oldFilePath;
            }

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, subFolder ?? "images");
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation("Uploads folder: {UploadsFolder}", uploadsFolder);

                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    var oldFileName = Path.GetFileName(oldFilePath);
                    var oldFullPath = Path.Combine(uploadsFolder, oldFileName);

                    if (File.Exists(oldFullPath))
                    {
                        _logger.LogInformation("Deleting old file: {OldFullPath}", oldFullPath);
                        File.Delete(oldFullPath);
                    }
                    else
                    {
                        _logger.LogWarning("Old file not found: {OldFullPath}", oldFullPath);
                    }
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newFile.FileName)}";
                var newPath = Path.Combine(uploadsFolder, uniqueFileName);

                _logger.LogInformation("Saving new file: {NewPath}", newPath);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await newFile.CopyToAsync(stream);
                }

                var relativePath = Path.Combine(subFolder ?? "images", uniqueFileName).Replace("\\", "/");
                _logger.LogInformation("File updated successfully: {RelativePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file");
                throw;
            }
        }
    }
}
