namespace EmployeeAPI.Services.DepartmentServices
{
    public static class ResponseModel
    {
        public record DepartmentDto
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public record CreateDepartment
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
        }
        public record UpdateDepartment
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

        public class UserFilter
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public double BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
