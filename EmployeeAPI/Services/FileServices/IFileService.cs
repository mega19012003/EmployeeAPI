namespace EmployeeAPI.Services.FileServices
{
    public interface IFileService
    {
        Task<List<string>> SaveFileAsync(List<IFormFile> files, string folderPath);
        //Task<List<string>> UpdateFileAsync(List<IFormFile> files, string folderPath, List<string> oldFiles);
        Task<List<string>> GetImagePath(int id, string folderPathPath, List<string> files);

    }
}
