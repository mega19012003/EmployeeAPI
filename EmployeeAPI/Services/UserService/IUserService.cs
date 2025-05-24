using EmployeeAPI.Base;
using EmployeeAPI.Services.AuthServices;

namespace EmployeeAPI.Services.UserService
{
    public interface IUserService
    {
        Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.UpdateUser dto);
        Task<string> SoftDeleteAsync(Guid id);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);
    }
}
