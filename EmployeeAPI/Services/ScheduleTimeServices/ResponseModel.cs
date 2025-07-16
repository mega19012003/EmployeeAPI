using EmployeeAPI.Models;
using System.Text.Json.Serialization;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public class ResponseModel
    {
        public class ScheduleDto
        {
            public Guid id { get; set; }
            public TimeOnly StartTimeMorning { get; set; }
            public TimeOnly EndTimeMorning { get; set; }
            public int LogAllowtime { get; set; }
            public TimeOnly StartTimeAfternoon { get; set; }
            public TimeOnly EndTimeAfternoon { get; set; }
            public string CompanyName { get; set; }
        }

        public class UpdateSchedule
        {
            public Guid id { get; set; }
            public TimeOnly StartTimeMorning { get; set; }
            public TimeOnly EndTimeMorning { get; set; }
            public int LogAllowtime { get; set; }
            public TimeOnly StartTimeAfternoon { get; set; }
            public TimeOnly EndTimeAfternoon { get; set; }
        }
    }
}
