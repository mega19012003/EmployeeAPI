using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public interface IAllowedIPService
    {
        Task<PagedResult<ResponseModel.IPDto>> GetAllAllowedIPsAsync(string? ip, int? pageIndex, int? pageSize);
        Task<bool> IsIpAllowedAsync(string ipAddress);
        Task<ResponseModel.IPDto> AddAllowedIPAsync(string ipAddress);
        Task<ResponseModel.IPDto> UpdateAllowedIPAsync(ResponseModel.IPDto dto);
        Task<string> DeleteAllowedIPAsync(Guid IPId);
    }
}
