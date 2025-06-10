using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EmployeeAPI.Base;
using EmployeeAPI.Models;

namespace EmployeeAPI.Services.DutyServices
{
    public static class ResponseModel
    {
        public class DutyDto //: BaseDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public Guid AssignedById { get; set; }
            public string AssignedBy{ get; set; }
            public bool IsCompleted { get; set; }
            public bool IsDeleted { get; set; }
            public List<DutyDetailDto> DutyDetails { get; set; } = new List<DutyDetailDto>();
        }
        public class CreateDuty
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public List<CreateDutyDetail> DutyDetails { get; set; } = new List<CreateDutyDetail>();
        }

        public class GetDutyDto
        {
            public Guid Id { get; set; }
            public List<CreateDutyDetail> DutyDetails { get; set; } = new List<CreateDutyDetail>();
        }
        public class UpdateDuty
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            public bool IsCompleted { get; set; }
            //public List<UpdateDutyDetail> DutyDetails { get; set; } = new List<UpdateDutyDetail>();
        }

        /*public class DeleteDuty
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; }
        }*/
        public class DutyDetailDto //: BaseDto
        {
            [Key]
            public Guid DutyDetailId { get; set; }
            public Guid userId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreateDutyDetail //: BaseDto
        {
            public Guid userId { get; set; }
            public string Description { get; set; }
        }
        public class UpdateDutyDetail
        {
            public Guid DutyDetailId { get; set; }
            public Guid userId { get; set; }
            public string Description { get; set; }
        }
    }
}
