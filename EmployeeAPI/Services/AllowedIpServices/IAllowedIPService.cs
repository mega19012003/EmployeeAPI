using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public interface IAllowedIPService
    {
        Task<PagedResult<AllowedIP>> GetAllAsync(string? IpAdress, int? pageIndex, int? pageSize);
        Task<AllowedIP> GetByIdAsync(Guid id);
        Task<AllowedIP> AddAsync(string ip);
        Task<string> DeleteAsync(Guid id);
        Task<bool> IsIPAllowedAsync(string ip);
    }
}
