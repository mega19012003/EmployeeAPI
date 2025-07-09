using System.ComponentModel.DataAnnotations;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User //: BaseEntity
    {
        [Key]
        public Guid UserId { get; set; }
        //[Required(ErrorMessage = "Username không được để trống")]
        [MaxLength(20, ErrorMessage = "Username không được dài quá 20 ký tự")]
        public string Username { get; set; }
        public string Password { get; set; }
        [MaxLength(100, ErrorMessage = "Họ tên không được dài quá 100 ký tự")]
        public string Fullname { get; set; }
        public RoleType Role { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(11, ErrorMessage = "Số điện thoại không được dài quá 11 ký tự")]
        public string? PhoneNumber { get; set; }
        [MaxLength(200, ErrorMessage = "Địa chỉ không được dài quá 200 ký tự")]
        public string? Address { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department Department { get; set; }
        public Guid? PositionId { get; set; }
        public Position Position { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản phải >= 0")]
        public double SalaryPerHour { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } 
        public string? ImageUrl { get; set; }
        // 1. Người giao việc (manager)
        public ICollection<Duty> AssignedDuties { get; set; } = new List<Duty>();

        // 2. Người được giao việc (employee)
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();
        public List<Checkin>? Checkins { get; set; } = new List<Checkin>();
        public List<Payroll>? Payrolls { get; set; } = new List<Payroll>();
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; } 
        public int TokenVersion { get; set; } = 0;
        //public string note { get; set; } = string.Empty;
    }
}
