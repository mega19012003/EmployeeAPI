using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
            //public RoleType Role { get; set; } //= RoleType.Employee;
            public string RoleName { get; set; }
            //public DateOnly DateOfBirth { get; set; }
            //public string PhoneNumber { get; set; }
            //public string Address { get; set; }
            //public string DepartmentName { get; set; }
            //public string PositionName { get; set; }
            //public double BasicSalary { get; set; }
            //public string ImageUrl { get; set; }
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
            public string Password { get; set; }
            [Required]
            public string Fullname { get; set; }
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
