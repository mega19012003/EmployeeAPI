using EmployeeAPI.Base;

namespace EmployeeAPI.Models
{
    public class Holiday //: BaseEntity
    {
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
