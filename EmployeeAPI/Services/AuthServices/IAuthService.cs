using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.AuthServices
{
    public interface IAuthService
    {
        Task<ResponseModel.AuthDto> RegisterAsync(ResponseModel.RegisterDto dto, ClaimsPrincipal user);
        Task<User> LoginAsync(string username, string password);
        /*Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.UpdateUser dto);
        Task<string> SoftDeleteAsync(Guid id);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);*/
        Task<ResponseModel.AuthDto> GetLoginUserAsync(ResponseModel.GetUserLogin dto);
        //Task<IEnumerable<ResponseModel.UserDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex);
    }
}
