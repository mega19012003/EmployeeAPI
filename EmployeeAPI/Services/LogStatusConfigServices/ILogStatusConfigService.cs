using EmployeeAPI.Models;

namespace EmployeeAPI.Services.CheckinStatusConfigServices
{
    public interface ILogStatusConfigService
    {
        Task<IEnumerable<LogStatusConfig>> GetAllConfigsAsync();

        Task<LogStatusConfig?> GetConfigAsync(int id);

        Task<LogStatusConfig> UpdateConfigAsync(LogStatusConfig updatedConfig);

    }
}
