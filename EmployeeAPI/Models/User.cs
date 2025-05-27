using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 
        public string Fullname { get; set; }
        public RoleType Role { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department Department { get; set; }
        public Guid? PositionId { get; set; }
        public Position Position { get; set; }
        public double BasicSalary { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? ImageUrl { get; set; }
        // 1. Người giao việc (manager)
        public ICollection<Duty> AssignedDuties { get; set; } = new List<Duty>();

        // 2. Người được giao việc (employee)
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();
        public List<Checkin>? Checkins { get; set; } = new List<Checkin>();
        public List<Payroll>? Payrolls { get; set; } = new List<Payroll>();
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; } 
        //public string note { get; set; } = string.Empty;
    }
}
