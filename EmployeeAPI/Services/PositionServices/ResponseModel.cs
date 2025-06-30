using EmployeeAPI.Base;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class ResponseModel
    {
        public class PositionDTO
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string DepartmentName { get; set; }
        }
        public class CreatePositionDto
        {
            //public Guid PositionId { get; set; }
            public Guid? DepartmentId { get; set; }
            public string Name { get; set; }
        }
        public class UpdatePositionDto
        {
            public Guid PositionId { get; set; }
            public string Name { get; set; }
        }
        public class DeletePositionDto
        {
            public Guid Id { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class UserFilterDto
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string Position { get; set; }
            public double BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
