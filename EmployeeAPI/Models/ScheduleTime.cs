using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Models
{
    public class ScheduleTime
    {
        [Key]
        [JsonIgnore]
        public Guid id { get; set; }
        public TimeOnly StartTimeMorning { get; set; } 
        public TimeOnly EndTimeMorning { get; set; }
        public int LogAllowtime { get; set; }
         //public int LateThresholdMinutes { get; set; } 
        public TimeOnly StartTimeAfternoon { get; set; }
        public TimeOnly EndTimeAfternoon { get; set; }

        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }
        public bool IsSystemDefault { get; set; }
    }
}
