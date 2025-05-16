using EmployeeAPI.Enums;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.CheckinServices
{
    public class ResponseModel
    {
        public record CheckinDto
        {
            public Guid CheckinId { get; set; }
            public Guid StaffId { get; set; }
            public DateTime CheckinDate { get; set; }
            public CheckinStatus Status { get; set; }
            //public Guid PayrollId { get; set; } //lương tháng này
        }
        public record CreateCheckin
        {
            //public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public DateTime CheckinDate { get; set; }
            public CheckinStatus Status { get; set; } 
            //public Guid PayrollId { get; set; } 
        }

        public record UpdateCheckin
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public DateTime CheckinDate { get; set; }
            public CheckinStatus Status { get; set; }
            //public Guid PayrollId { get; set; } //lương tháng này
        }

        /*public record DeleteCheckin
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public DateTime CheckinDate { get; set; }
            public bool IsDeleted { get; set; } = false;
        }*/
    }
}
