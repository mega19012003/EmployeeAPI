using EmployeeAPI.Models;

namespace EmployeeAPI.Models
{
    public class Staff
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
        public Guid PositionId { get; set; }
        public Position Position { get; set; }
        public double BasicSalary { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public List<string> ImageUrl { get; set; }
        //public ICollection<Fine> Fines { get; set; }
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();
        public List<Checkin> Checkins { get; set; } = new List<Checkin>();
        public List<Payroll> Payrolls { get; set; } = new List<Payroll>();
        /*public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();*/
    }
}
