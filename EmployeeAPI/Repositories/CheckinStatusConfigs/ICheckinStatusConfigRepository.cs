using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.CheckinStatusConfigs
{
    public interface ICheckinStatusConfigRepository
    {
        Task<List<CheckinStatusConfig>> GetAllAsync();
        Task<CheckinStatusConfig?> GetByIdAsync(int id);
        Task UpdateAsync(CheckinStatusConfig config);
    }
}
