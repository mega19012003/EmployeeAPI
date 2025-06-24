using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.CheckinStatusConfigs
{
    public interface ILogStatusConfigRepository
    {
        Task<List<LogStatusConfig>> GetAllAsync();
        Task<LogStatusConfig?> GetByIdAsync(int id);
        Task UpdateAsync(LogStatusConfig config);
    }
}
