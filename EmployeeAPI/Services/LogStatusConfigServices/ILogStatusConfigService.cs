using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Services.LogStatusConfigServices;

namespace EmployeeAPI.Services.LogStatusConfigServices
{
    public interface ILogStatusConfigService
    {
        Task<PagedResult<ResponseModel.LogStatusDto>> GetAllConfigsAsync(string? name, Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles);

        Task<ResponseModel.LogStatusDto> GetConfigIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);

        Task<ResponseModel.LogStatusDto> UpdateConfigAsync(ResponseModel.UpdateLogStatusDto updatedConfig);

    }
}
