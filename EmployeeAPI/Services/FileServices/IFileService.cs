namespace EmployeeAPI.Services.FileServices
{
    public interface IFileService
    {
        Task<List<string>> SaveFilesAsync(List<IFormFile> files, string uploadsFolder);
        //Task<List<string>> UpdateFileAsync(List<IFormFile> files, string folderPath, List<string> oldFiles);

    }
}
