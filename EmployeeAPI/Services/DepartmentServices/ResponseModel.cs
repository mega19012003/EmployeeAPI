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
    }
}
