using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.AuthServices.ResponseModel;

namespace EmployeeAPI.Services.AuthServices
{
    public interface IAuthService
    {
        Task<User> GetUserById(Guid userId);
        Task<string> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<ResponseModel.AuthDto> RegisterAsync(ResponseModel.RegisterDto dto, ClaimsPrincipal user);
        Task<User> LoginAsync(string username, string password);
        Task<string> ChangePasswordAsync(Guid userId, string oldPassword, string confirmPassword, string newPassword);
        Task<string> ResetPasswordAsync(Guid userId, ClaimsPrincipal claim);
        /*Task<ResponseModel.UserDto> UpdateAsync(ResponseModel.UpdateUser dto);
        Task<string> SoftDeleteAsync(Guid id);

        Task<PagedResult<ResponseModel.UserDto>> GetAllAsync(string? SearchTerm, Guid? departmentId, int? pageSize, int? pageIndex);
        Task<ResponseModel.UserDto> GetByIdAsync(Guid id);*/
        Task LogoutAsync(Guid userId);
        Task<ResponseModel.AuthDto> GetLoginUserAsync(ResponseModel.GetUserLogin dto);
        //Task<IEnumerable<ResponseModel.UserDto>> GetByNameAsync(string name, int? pageSize, int? pageIndex);

    }
}
