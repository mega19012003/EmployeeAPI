
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinDate { get; set; } 
        public CheckinStatus Status { get; set; } = CheckinStatus.OnTime;
        public bool IsDeleted { get; set; } = false;
    }
}
