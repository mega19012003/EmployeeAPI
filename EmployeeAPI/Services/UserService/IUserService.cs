using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Services.UserService
{
    public interface IUserService
    {
        //Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid? companyId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize);
        Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid? companyId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize, int? Month);
        Task<PagedResult<ResponseModel.UserResultDto>> GetActiveEmployeesAndManagersAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid? companyId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize);
        Task<ResponseModel.UserResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ResponseModel.UserResultDto> UpdateStaffAsync(ResponseModel.UpdateDto dto, Guid currentUserId, IList<string> currentUserRoles);
        Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
    }
}
