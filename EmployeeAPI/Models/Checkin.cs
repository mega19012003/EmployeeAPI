using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinMorning { get; set; } 
        public DateTime CheckoutMorning { get; set; }
        public DateTime CheckinAfternoon { get; set; }
        public DateTime CheckoutAfternoon { get; set; }
        public Enums.LogStatus? CheckinMorningStatus { get; set; } 
        public Enums.LogStatus? CheckoutMorningStatus { get; set; }
        public Enums.LogStatus? CheckinAfternoonStatus { get; set; }
        public Enums.LogStatus? CheckoutAfternoonStatus { get; set; }
        //public double SalaryPerDay { get; set; } 
        public bool IsDeleted { get; set; } = false;
    }
}
