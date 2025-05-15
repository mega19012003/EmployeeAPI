
namespace EmployeeAPI.Models
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public Staff Staff { get; set; }
        public DateTime CheckinDate { get; set; }
        public TimeOnly CheckinTime { get; set; }
        public bool IsHoliday { get; set; } = false; //có phải ngày nghỉ lễ
        public bool IsOvertime { get; set; } = false; //có tăng ca
        public bool isLate { get; set; } = false;  // có đi trễ ko ?!?!?
        public Guid PayrollId { get; set; } //lương tháng này
        public Payroll Payroll { get; set; } //lương tháng này
        //public double TotalHours { get; set; }
        //public bool Overtime { get; set; } = false;
        //public double OvertimeLenght { get; set; }
        //public double? TotalLeave { get; set; }
    }
}
