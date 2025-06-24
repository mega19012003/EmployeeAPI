
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinDate { get; set; } 
        public DateTime CheckoutDate { get; set; } 
        public Enums.LogStatus CheckinStatus { get; set; } = Enums.LogStatus.OnTime;
        public Enums.LogStatus CheckoutStatus { get; set; } = Enums.LogStatus.OnTime;
        public double SalaryPerDay { get; set; } = 0.0;
        public bool IsDeleted { get; set; } = false;
        //public string updateBy { get; set; } 
        //public DateTime UpdateAt { get; set; }
    }
}
