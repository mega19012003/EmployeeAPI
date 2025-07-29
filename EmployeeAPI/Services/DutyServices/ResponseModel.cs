using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.DutyServices
{
    public static class ResponseModel
    {
        public class DutyResultDto 
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            [JsonIgnore]
            public Guid AssignedById { get; set; }
            public string AssignedBy { get; set; }
            public string AssignImageUrl { get; set; }
            public string Status { get; set; }
            //public bool IsCompleted { get; set; }
            [JsonIgnore]
            public bool IsDeleted { get; set; }
            public List<DutyDetailResultDto> DutyDetails { get; set; } = new List<DutyDetailResultDto>();
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            public string CompanyName { get; set; }
        }
        public class DutyDetailResultDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string UserImageUrl { get; set; }
            public string Status { get; set; }
            public DateOnly Deadline { get; set; }
            //public bool IsCompleted { get; set; }
            public string Description { get; set; }
        }
        public class CreateDutyDto
        {
            public string Name { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public List<CreateDutyDetailDto> DutyDetails { get; set; } = new List<CreateDutyDetailDto>();
        }

        public class GetDutyDto
        {
            public Guid Id { get; set; }
            public List<CreateDutyDetailDto> DutyDetails { get; set; } = new List<CreateDutyDetailDto>();
        }
        public class UpdateDutyDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            //public bool IsCompleted { get; set; }
        }

        public class CreateDutyDetailDto
        {
            public Guid userId { get; set; }
            public string Description { get; set; }
            public DateOnly Deadline { get; set; }
        }
        public class UpdateDutyDetailDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid userId { get; set; }
            public string Description { get; set; }
            public DateOnly Deadline { get; set; }
        }

        //public class MarkAsCompleted 
        //{
        //    public Guid Id { get; set; }
        //    public string Name { get; set; }
        //    public bool IsCompleted { get; set; }
        //}
    }
}
