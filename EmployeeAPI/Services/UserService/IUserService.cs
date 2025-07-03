using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Services.UserService
{
    public interface IUserService
    {
      
        //Task<ResponseModel.UserResultDto> UpdateAsync(ResponseModel.AdminUpdateDto userRole, Guid currentUserId);
        //Task<ResponseModel.UserResultDto> AdminUpdateStaffAsync(ResponseModel.AdminUpdateDto dto/*, ClaimsPrincipal user*/);
        Task<ResponseModel.UserResultDto> UpdateStaffAsync(ResponseModel.AdminUpdateDto dto, Guid currentUserId, IList<string> currentUserRoles);
        //Task<ResponseModel.UserResultDto> ManagerUpdateStaffAsync(ResponseModel.ManagerUpdateDto dto, Guid managerId/*, ClaimsPrincipal user*/);
        Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);

        Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize);
        //Task<ResponseModel.UserResultDto> GetAllUser();
        Task<ResponseModel.UserResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        //Task<IQueryable<ResponseModel.UserResultDto>> GetAllUser(Guid currentUserId, IList<string> currentUserRoles, Guid? departmentId);
    }
}
