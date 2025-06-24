using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.AuthServices
{
    public class ResponseModel
    {
        public class AuthDto 
        {
            public Guid userId { get; set; }
            public String Username { get; set; }
            public string Fullname { get; set; }
            public string Password { get; set; }
            public string RoleName { get; set; }
        }
        public class LoginDto
        {
            [Required]
            public string Username { get; set; }
            [Required]
            public string Password { get; set; }
        }
        public class ChangePasswordDto
        {
            [Required]
            public string OldPassword { get; set; }
            [Required]
            public string NewPassword { get; set; }
            [Required]
            public string ConfirmPassword { get; set; }
        }
        public class ResetPasswordDto
        {
            [Required]
            public Guid UserId { get; set; }
        }
        public class RegisterDto
        {
            [Required]
            public string Username { get; set; }
            [Required]
            public string Fullname { get; set; }
            public string Password { get; set; }
            [Required]
            public RoleType Role { get; set; }
        }

        public class GetUserLogin 
        {
            public string UserName { get; set; }
        }
        public class RefreshTokenDto
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
