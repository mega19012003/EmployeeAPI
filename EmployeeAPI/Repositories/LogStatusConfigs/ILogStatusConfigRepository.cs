using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.LogStatusConfigs
{
    public interface ILogStatusConfigRepository
    {
        Task<List<LogStatusConfig>> GetTemplateAsync();
        Task<List<LogStatusConfig>> GetAllAsync(Guid companyId);
        Task<LogStatusConfig?> GetByIdAsync(Guid id);
        Task UpdateAsync(LogStatusConfig config);
    }
}
