using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.AllowedIPs
{
    public interface IAllowedIPRepository
    {
        Task<bool> IsIpAllowedAsync(string ipAddress);
        Task AddAllowedIPAsync(string ipAddress);
        Task <IEnumerable<AllowedIP>> GetAllAllowedIPsAsync();
    }
}
