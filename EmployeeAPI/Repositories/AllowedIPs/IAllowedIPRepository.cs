using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.AllowedIPs
{
    public interface IAllowedIPRepository
    {
        Task<List<AllowedIP>> GetAllAsync();
        Task<AllowedIP> GetByIdAsync(Guid id);
        Task AddAsync(AllowedIP entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string ip);
    }
}
