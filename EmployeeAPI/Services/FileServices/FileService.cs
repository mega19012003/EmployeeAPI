namespace EmployeeAPI.Services.FileServices;

public class FileService : IFileService
{
    public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string uploadsFolder)
    {
        var imagePaths = new List<string>();

        if (files == null || files.Count == 0)
            return imagePaths;

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // Tạo tên file duy nhất (ví dụ: GUID + tên gốc)
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                // Đường dẫn lưu file
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào đĩa
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Thêm đường dẫn dạng "images/filename.ext" để lưu vào DB
                var relativePath = Path.Combine("images", uniqueFileName).Replace("\\", "/");
                imagePaths.Add(relativePath);
            }
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

