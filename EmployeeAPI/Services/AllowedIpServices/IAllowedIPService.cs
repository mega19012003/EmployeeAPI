using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public interface IAllowedIPService
    {
        Task<bool> IsIpAllowedAsync(string ipAddress);
        Task<AllowedIP> AddAllowedIPAsync(string ipAddress);
        Task<PagedResult<AllowedIP>> GetAllAllowedIPsAsync(string? ip, int? pageIndex, int? pageSize);
    }
}
