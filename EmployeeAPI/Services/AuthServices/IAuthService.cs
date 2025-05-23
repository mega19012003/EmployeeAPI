using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.AuthServices
{
    public interface IAuthService
    {
        Task<ResponseModel.UserDto> RegisterAsync(ResponseModel.RegisterDto dto);
        Task<User> LoginAsync(string username, string password);
        Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.UpdateUser dto);
        Task<string> SoftDeleteAsync(Guid id);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);
        Task<ResponseModel.UserDto> GetLoginUserAsync(ResponseModel.UserDto dto);
        //Task<IEnumerable<ResponseModel.UserDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex);
    }
}
