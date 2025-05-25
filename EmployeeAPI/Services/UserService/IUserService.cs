using EmployeeAPI.Base;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Services.UserService
{
    public interface IUserService
    {
      
        //Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.AdminUpdateDto userRole, Guid currentUserId);
        Task<ResponseModel.UserDto> AdminUpdateStaffAsync(ResponseModel.AdminUpdateDto dto);
        Task<ResponseModel.UserDto> ManagerUpdateStaffAsync(ResponseModel.ManagerUpdateDto dto, Guid managerId);
        Task<string> SoftDeleteAsync(Guid id);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);
    }
}
