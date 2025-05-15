using System.Text.Json.Serialization;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.DutyServices
{
    public static class ResponseModel
    {
        public record DutyDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public bool IsCompleted { get; set; }
            public List<DutyDetailDto> DutyDetails { get; set; } = new List<DutyDetailDto>();
        }
        public record CreateDuty
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public List<DutyDetailDto> DutyDetails { get; set; } = new List<DutyDetailDto>();
        }
        public record UpdateDuty
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
            public List<UpdateDutyDetail> DutyDetails { get; set; } = new List<UpdateDutyDetail>();
        }

        public record DeleteDuty
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; }
        }
        public record DutyDetailDto
        {
            public Guid StaffId { get; set; }
            public string Description { get; set; }
        }
        public record UpdateDutyDetail
        {
            public Guid Id { get; set; }
            public Guid StaffId { get; set; }
            public string Description { get; set; }
        }
    }
}
