
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public Staff Staff { get; set; }
        public DateTime CheckinDate { get; set; }
        public CheckinStatus Status { get; set; } = CheckinStatus.OnTime;
        /*public Guid PayrollId { get; set; } //lương tháng này
        public Payroll Payroll { get; set; } */
        public bool IsDeleted { get; set; } = false;
        //public double TotalHours { get; set; }
        //public bool Overtime { get; set; } = false;
        //public double OvertimeLenght { get; set; }
        //public double? TotalLeave { get; set; }
    }
}
