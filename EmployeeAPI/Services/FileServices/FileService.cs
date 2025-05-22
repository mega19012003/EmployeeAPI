namespace EmployeeAPI.Services.FileServices;

public class FileService : IFileService
{
    public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string uploadsFolder)
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
    }

    public async Task<List<string>> UpdateFilesAsync(List<IFormFile> files, string uploadsFolder, List<String>oldFiles)
    {
        var imagePaths = new List<string>();

        if (files == null || files.Count == 0)
            return imagePaths;

        try
        {
            Directory.CreateDirectory(uploadsFolder); 

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

            foreach (var oldFile in oldFiles)
            {
                var fullPath = Path.Combine(uploadsFolder, oldFile);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Lỗi lưu file: {ex.Message}");
            throw; 
        }

        return imagePaths;
    }


    /*public async Task<List<string>> UpdateFileAsync(List<IFormFile> files, string folderPath, List<String> oldFiles)
    {
        var filePaths = new List<string>();
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(folderPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                filePaths.Add(Path.Combine("images", uniqueFileName).Replace("\\", "/"));
            }
        }
        foreach (var oldFile in oldFiles)
        {
            var fullPath = Path.Combine(folderPath, oldFile);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        return filePaths;
    }*/

}

