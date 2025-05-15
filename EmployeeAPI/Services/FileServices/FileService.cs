namespace EmployeeAPI.Services.FileServices;

public class FileService : IFileService
{
    public async Task<List<string>> SaveFileAsync(List<IFormFile> files, string folderName)
    {
        List<string> savedPaths = new();

        // Đường dẫn tuyệt đối tới thư mục wwwroot/images (hoặc thư mục được chỉ định)
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folderPath = Path.Combine(rootPath, folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (var file in files)
        {
            var safeFileName = Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}_{safeFileName}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            savedPaths.Add(Path.Combine(folderName, fileName).Replace("\\", "/"));
        }

        return savedPaths;
    }

    public async Task<List<string>> GetImagePath(int id, string folderPath, List<string> files)
    {
        var filePaths = new List<string>();
        foreach (var file in files)
        {
            var fullPath = Path.Combine(folderPath, file);
            if (File.Exists(fullPath))
            {
                filePaths.Add(Path.Combine("images", file).Replace("\\", "/"));
            }
        }
        return filePaths;
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

