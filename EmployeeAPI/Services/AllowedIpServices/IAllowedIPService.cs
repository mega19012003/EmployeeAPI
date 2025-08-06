using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.AllowedIPs;

namespace EmployeeAPI.Services.AllowedIpServices
{
    public interface IAllowedIPService
    {
        Task<PagedResult<ResponseModel.IPDto>> GetAllAsync(string? IpAdress, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.IPDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.IPDto> AddAsync(string ip, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> DeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<bool> IsIPAllowedAsync(string ip, Guid companyId);
    }
}
