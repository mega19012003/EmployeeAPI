namespace EmployeeAPI.Services.FileServices;

public class FileService : IFileService
{
    public async Task<string?> SaveFileAsync(IFormFile file, string uploadsFolder)
    {
        if (file == null || file.Length == 0)
            return null;

        try
        {
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("images", uniqueFileName).Replace("\\", "/");
            return relativePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu file: {ex.Message}");
            throw;
        }
    }
    public async Task<string?> UpdateFileAsync(IFormFile newFile, string uploadsFolder, string? oldFilePath)
    {
        if (newFile == null || newFile.Length == 0)
            return oldFilePath;

        try
        {
            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(oldFilePath))
            {
                var oldFullPath = Path.Combine(uploadsFolder, Path.GetFileName(oldFilePath));
                if (File.Exists(oldFullPath))
                {
                    File.Delete(oldFullPath);
                }
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newFile.FileName)}";
            var newPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await newFile.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("images", uniqueFileName).Replace("\\", "/");
            return relativePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi cập nhật file: {ex.Message}");
            throw;
        }
    }

    /*public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string uploadsFolder)
    {
        var imagePaths = new List<string>();

        if (files == null || files.Count == 0)
            return imagePaths;

        try
        {
            Directory.CreateDirectory(uploadsFolder); // tạo thư mục nếu chưa có

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativePath = Path.Combine("images", uniqueFileName).Replace("\\", "/");
                    imagePaths.Add(relativePath);
                }
            }
        }
        catch (Exception ex)
        {
            // Ghi log hoặc trả lỗi rõ ràng để biết chuyện gì đang xảy ra
            Console.WriteLine($"Lỗi lưu file: {ex.Message}");
            throw; // hoặc return imagePaths để không ngắt tiến trình
        }

        return imagePaths;
    }*/

}

