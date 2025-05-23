using EmployeeAPI.Enums;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.CheckinServices
{
    public class ResponseModel
    {
        public record CheckinDto
        {
            public Guid CheckinId { get; set; }
            public Guid userId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinDate { get; set; }
            public CheckinStatus CheckinStatus { get; set; }
            public string Status { get; set; } 
        }
        public record CreateCheckin
        {
            //public Guid Id { get; set; }
            public Guid userId { get; set; }
            public DateTime CheckinDate { get; set; }
            public CheckinStatus CheckinStatus { get; set; }
        }

        public record UpdateCheckin
        {
            public Guid CheckinId { get; set; }
            //public Guid userId { get; set; }
            //public DateTime CheckinDate { get; set; }
            public CheckinStatus CheckinStatus { get; set; }
            public string Status { get; set; }
        }

    }
}
