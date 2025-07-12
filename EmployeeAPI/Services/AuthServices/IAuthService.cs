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
        Task LogoutAsync(Guid userId);
        Task<ResponseModel.AuthDto> GetLoginUserAsync(ResponseModel.GetUserLogin dto);
    }
}
