using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeAPI.Services.UserService
{
    public class ResponseModel
    {
        public class UserResultDto 
        {
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public string RoleName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string DepartmentName { get; set; }
            public Guid? DepartmentId { get; set; }
            public string PositionName { get; set; }
            public Guid? PositionId { get; set; }
            public double? BasicSalary { get; set; }
            public string ImageUrl { get; set; }
            public bool IsActive { get; set; } = true;
        }
        public class AdminUpdateDto 
        {
            [Required]
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public RoleType? Role { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public Guid? DepartmentId { get; set; }
            public Guid? PositionId { get; set; }
            //[Required]
            public double? BasicSalary { get; set; }
            public IFormFile? ImageUrl { get; set; }
            public bool? IsActive { get; set; }
        }

    }
}
