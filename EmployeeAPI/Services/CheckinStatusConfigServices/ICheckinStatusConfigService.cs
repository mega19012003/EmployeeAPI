using EmployeeAPI.Models;

namespace EmployeeAPI.Services.CheckinStatusConfigServices
{
    public interface ICheckinStatusConfigService
    {
        Task<IEnumerable<CheckinStatusConfig>> GetAllConfigsAsync();

        Task<CheckinStatusConfig?> GetConfigAsync(int id);

        Task<CheckinStatusConfig> UpdateConfigAsync(CheckinStatusConfig updatedConfig);

    }
}
