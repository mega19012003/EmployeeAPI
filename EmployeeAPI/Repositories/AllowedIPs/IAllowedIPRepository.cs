using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.AllowedIPs
{
    public interface IAllowedIPRepository
    {
        Task <IEnumerable<AllowedIP>> GetAllAllowedIPsAsync();
        Task<AllowedIP> GetAllowedIPAsync (Guid IPId);
        Task<bool> IsIpAllowedAsync(string ipAddress);
        Task AddAllowedIPAsync(string ipAddress);
        Task UpdateAllowedIpAsync(AllowedIP model);
        Task DeleteAllowedIPAsync(Guid IPId);
    }
}
