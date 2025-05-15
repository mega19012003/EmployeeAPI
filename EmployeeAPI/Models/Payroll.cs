

using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAPI.Models
{
    public class Payroll
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public Staff Staff { get; set; }
        public Double Salary { get; set; }
        public DateTime CheckoutDate { get; set; } //ngày trả lương
        public string Note { get; set; }
        //public virtual ICollection<Fine> Fines { get; set; }
        /*public ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();
        public ICollection<Fine> Fines { get; set; } = new List<Fine>();*/

        /*NotMapped]
        public double TotalCheckin
        {
            get
            {
                return Checkins.Count(c => c.CheckinDate.Date == CheckoutDate.Date);
            }
        }

        [NotMapped]
        public double TotalOvertime
        {
            get
            {
                return Checkins.Count(c => c.CheckinDate.Date == CheckoutDate.Date && c.IsOvertime);
            }
        }

        [NotMapped]
        public double TotalHollidayWork
        {
            get
            {
                return Checkins.Count(c => c.CheckinDate.Date == CheckoutDate.Date && c.IsHoliday);
            }
        }

        [NotMapped]
        public double TotalSalary
        {
            get
            {
                return (TotalCheckin * Staff.BasicSalary) + (Staff.BasicSalary * (30/100) * TotalOvertime) + (Staff.BasicSalary * (50/100) * TotalHollidayWork);
            }
        }*/
    }
}
