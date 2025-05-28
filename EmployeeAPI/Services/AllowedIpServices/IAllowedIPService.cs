using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public interface IAllowedIPService
    {
        Task<List<AllowedIP>> GetAllAsync();
        Task<AllowedIP> GetByIdAsync(Guid id);
        Task AddAsync(string ip);
        Task DeleteAsync(Guid id);
        Task<bool> IsIPAllowedAsync(string ip);
    }
}
