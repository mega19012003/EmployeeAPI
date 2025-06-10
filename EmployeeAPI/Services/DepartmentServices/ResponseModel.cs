using EmployeeAPI.Base;

namespace EmployeeAPI.Services.DepartmentServices
{
    public static class ResponseModel
    {
        public class DepartmentDto //: BaseDto
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreateDepartment //: BaseDto
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
        }
        public class UpdateDepartment //: BaseDto
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
            //public bool IsDeleted { get; set; }
        }

        public class PositionByDepartment
        {
            public Guid PositionId { get; set; }
            public string PositionName { get; set; }
            public string DepartmentName { get; set; }
        }

        public class UserFilter //: BaseDto
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public double BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
