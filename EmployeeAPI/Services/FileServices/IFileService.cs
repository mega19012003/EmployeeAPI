namespace EmployeeAPI.Services.FileServices
{
    public interface IFileService
    {
        Task<string?> SaveFileAsync(IFormFile file, string uploadsFolder);
        Task<string?> UpdateFileAsync(IFormFile newFile, string uploadsFolder, string? oldFilePath);
        //Task<string> UpdateFileAsync(List<IFormFile> files, string folderPath, List<string> oldFiles);

    }
}
