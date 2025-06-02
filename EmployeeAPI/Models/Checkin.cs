
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
        public CheckinStatus CheckinStatus { get; set; } = CheckinStatus.OnTime;
        public CheckinStatus CheckoutStatus { get; set; } = CheckinStatus.OnTime;
        public double SalaryPerDay { get; set; } = 0.0;
        public bool IsDeleted { get; set; } = false;
    }
}
