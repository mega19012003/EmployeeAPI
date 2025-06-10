using EmployeeAPI.Base;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class ResponseModel
    {
        public class PositionDTO //: BaseDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreatePosition
        {
            //public Guid PositionId { get; set; }
            public Guid? DepartmentId { get; set; }
            public string Name { get; set; }
        }
        public class UpdatePosition //: BaseDto
        {
            public Guid PositionId { get; set; }
            public string Name { get; set; }
        }
        public class DeletePosition
        {
            public Guid Id { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class UserFilter //: BaseDto
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string Position {  get; set; }
            public double BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
