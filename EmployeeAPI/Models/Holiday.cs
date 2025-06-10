using EmployeeAPI.Base;

namespace EmployeeAPI.Models
{
    public class Holiday //: BaseEntity
    {
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
