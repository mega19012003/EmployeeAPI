using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class SalaryRule
    {
        public string SalaryRuleId { get; set; }
        public CheckinStatus CheckinStatus { get; set; }
        public double multiplier { get; set; }
        public string? Note { get; set;}
        public DateTime? Updated { get; set; }
    }
}
