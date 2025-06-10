using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Services.UserService
{
    public interface IUserService
    {
      
        //Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.AdminUpdateDto userRole, Guid currentUserId);
        Task<ResponseModel.UserDto> AdminUpdateStaffAsync(ResponseModel.AdminUpdateDto dto, ClaimsPrincipal user);
        Task<ResponseModel.UserDto> ManagerUpdateStaffAsync(ResponseModel.ManagerUpdateDto dto, Guid managerId, ClaimsPrincipal user);
        Task<string> SoftDeleteAsync(Guid id, ClaimsPrincipal user);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        //Task<ResponseModel.UserDto> GetAllUser();
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);
        Task<IQueryable<ResponseModel.UserDto>> GetAllUser();
    }
}
