

using System.ComponentModel.DataAnnotations.Schema;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Payroll
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public double Salary { get; set; }
        public DateTime CreatedDate { get; set; } 
        //public DateTime CheckinAfternoon { get; set; } //ngày trả lương
        public string Note { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int DaysWorked { get; set; } //ngày công
        //public CheckinMorningStatusConfig SalaryRule { get; set; }
        //public CheckinMorningStatus CheckinMorningStatus { get; set; }
        //public bool IsPaid { get; set; } = false; //đã trả lương
        //public List<Checkin> Checkins { get; set; } = new List<Checkin>();
        //public virtual ICollection<Fine> Fines { get; set; }
        /*public ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();*/
    }
}
