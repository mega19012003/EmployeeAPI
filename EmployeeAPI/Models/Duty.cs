using System.Text.Json.Serialization;
using EmployeeAPI.Models;

namespace EmployeeAPI.Models
{
    public class Duty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        //public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsDeleted { get; set; } = false; //xóa mềm
                                                     //public Guid StaffId { get; set; }
                                                     //public List<Staff> Staffs { get; set; }
                                                     //public bool OverTime { get; set; } = false;
                                                     //public bool IsDeleted { get; set; } = false; //xóa mềm
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();
    }
}
